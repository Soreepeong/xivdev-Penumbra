using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using Penumbra.GameData.ByteString;
using Penumbra.GameData.Enums;
using Penumbra.Interop.Structs;
using FileMode = Penumbra.Interop.Structs.FileMode;
using ResourceHandle = FFXIVClientStructs.FFXIV.Client.System.Resource.Handle.ResourceHandle;

namespace Penumbra.Interop.Loader;

public unsafe partial class ResourceLoader
{
    // Resources can be obtained synchronously and asynchronously. We need to change behaviour in both cases.
    // Both work basically the same, so we can reduce the main work to one function used by both hooks.
    public delegate ResourceHandle* GetResourceSyncPrototype( ResourceManager* resourceManager, ResourceCategory* pCategoryId,
        ResourceType* pResourceType, int* pResourceHash, byte* pPath, void* pUnknown );

    [Signature( "E8 ?? ?? 00 00 48 8D 8F ?? ?? 00 00 48 89 87 ?? ?? 00 00", DetourName = "GetResourceSyncDetour" )]
    public Hook< GetResourceSyncPrototype > GetResourceSyncHook = null!;

    public delegate ResourceHandle* GetResourceAsyncPrototype( ResourceManager* resourceManager, ResourceCategory* pCategoryId,
        ResourceType* pResourceType, int* pResourceHash, byte* pPath, void* pUnknown, bool isUnknown );

    [Signature( "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00", DetourName = "GetResourceAsyncDetour" )]
    public Hook< GetResourceAsyncPrototype > GetResourceAsyncHook = null!;

    private ResourceHandle* GetResourceSyncDetour( ResourceManager* resourceManager, ResourceCategory* categoryId, ResourceType* resourceType,
        int* resourceHash, byte* path, void* unk )
        => GetResourceHandler( true, resourceManager, categoryId, resourceType, resourceHash, path, unk, false );

    private ResourceHandle* GetResourceAsyncDetour( ResourceManager* resourceManager, ResourceCategory* categoryId, ResourceType* resourceType,
        int* resourceHash, byte* path, void* unk, bool isUnk )
        => GetResourceHandler( false, resourceManager, categoryId, resourceType, resourceHash, path, unk, isUnk );

    private ResourceHandle* CallOriginalHandler( bool isSync, ResourceManager* resourceManager, ResourceCategory* categoryId,
        ResourceType* resourceType, int* resourceHash, byte* path, void* unk, bool isUnk )
        => isSync
            ? GetResourceSyncHook.Original( resourceManager, categoryId, resourceType, resourceHash, path, unk )
            : GetResourceAsyncHook.Original( resourceManager, categoryId, resourceType, resourceHash, path, unk, isUnk );


    [Conditional( "DEBUG" )]
    private static void CompareHash( int local, int game, Utf8GamePath path )
    {
        if( local != game )
        {
            PluginLog.Warning( "Hash function appears to have changed. Computed {Hash1:X8} vs Game {Hash2:X8} for {Path}.", local, game, path );
        }
    }

    private event Action< Utf8GamePath, FullPath?, object? >? PathResolved;

    private ResourceHandle* GetResourceHandler( bool isSync, ResourceManager* resourceManager, ResourceCategory* categoryId,
        ResourceType* resourceType, int* resourceHash, byte* path, void* unk, bool isUnk )
    {
        if( !Utf8GamePath.FromPointer( path, out var gamePath ) )
        {
            PluginLog.Error( "Could not create GamePath from resource path." );
            return CallOriginalHandler( isSync, resourceManager, categoryId, resourceType, resourceHash, path, unk, isUnk );
        }

        CompareHash( gamePath.Path.Crc32, *resourceHash, gamePath );

        ResourceRequested?.Invoke( gamePath, isSync );

        // If no replacements are being made, we still want to be able to trigger the event.
        var (resolvedPath, data) = ResolvePath( gamePath, *categoryId, *resourceType, *resourceHash );
        PathResolved?.Invoke( gamePath, resolvedPath, data );
        if( resolvedPath == null )
        {
            var retUnmodified = CallOriginalHandler( isSync, resourceManager, categoryId, resourceType, resourceHash, path, unk, isUnk );
            ResourceLoaded?.Invoke( ( Structs.ResourceHandle* )retUnmodified, gamePath, null, data );
            return retUnmodified;
        }

        // Replace the hash and path with the correct one for the replacement.
        *resourceHash = resolvedPath.Value.InternalName.Crc32;
        path          = resolvedPath.Value.InternalName.Path;
        var retModified = CallOriginalHandler( isSync, resourceManager, categoryId, resourceType, resourceHash, path, unk, isUnk );
        ResourceLoaded?.Invoke( ( Structs.ResourceHandle* )retModified, gamePath, resolvedPath.Value, data );
        return retModified;
    }


    // Use the default method of path replacement.
    public static (FullPath?, object?) DefaultResolver( Utf8GamePath path )
    {
        var resolved = Penumbra.CollectionManager.Default.ResolvePath( path );
        return ( resolved, null );
    }

    // Try all resolve path subscribers or use the default replacer.
    private (FullPath?, object?) ResolvePath( Utf8GamePath path, ResourceCategory category, ResourceType resourceType, int resourceHash )
    {
        if( !DoReplacements )
        {
            return ( null, null );
        }

        path = path.ToLower();
        if( ResolvePathCustomization != null )
        {
            foreach( var resolver in ResolvePathCustomization.GetInvocationList() )
            {
                if( ( ( ResolvePathDelegate )resolver ).Invoke( path, category, resourceType, resourceHash, out var ret ) )
                {
                    return ret;
                }
            }
        }

        return DefaultResolver( path );
    }


    // We need to use the ReadFile function to load local, uncompressed files instead of loading them from the SqPacks.
    public delegate byte ReadFileDelegate( ResourceManager* resourceManager, SeFileDescriptor* fileDescriptor, int priority,
        bool isSync );

    [Signature( "E8 ?? ?? ?? ?? 84 C0 0F 84 ?? 00 00 00 4C 8B C3 BA 05" )]
    public ReadFileDelegate ReadFile = null!;

    // We hook ReadSqPack to redirect rooted files to ReadFile.
    public delegate byte ReadSqPackPrototype( ResourceManager* resourceManager, SeFileDescriptor* pFileDesc, int priority, bool isSync );

    [Signature( "E8 ?? ?? ?? ?? EB 05 E8 ?? ?? ?? ?? 84 C0 0F 84 ?? 00 00 00 4C 8B C3", DetourName = "ReadSqPackDetour" )]
    public Hook< ReadSqPackPrototype > ReadSqPackHook = null!;

    private byte ReadSqPackDetour( ResourceManager* resourceManager, SeFileDescriptor* fileDescriptor, int priority, bool isSync )
    {
        if( !DoReplacements )
        {
            return ReadSqPackHook.Original( resourceManager, fileDescriptor, priority, isSync );
        }

        if( fileDescriptor == null || fileDescriptor->ResourceHandle == null )
        {
            PluginLog.Error( "Failure to load file from SqPack: invalid File Descriptor." );
            return ReadSqPackHook.Original( resourceManager, fileDescriptor, priority, isSync );
        }

        if( !Utf8GamePath.FromSpan( fileDescriptor->ResourceHandle->FileNameSpan(), out var gamePath, false ) || gamePath.Length == 0 )
        {
            return ReadSqPackHook.Original( resourceManager, fileDescriptor, priority, isSync );
        }

        // Paths starting with a '|' are handled separately to allow for special treatment.
        // They are expected to also have a closing '|'.
        if( ResourceLoadCustomization == null || gamePath.Path[ 0 ] != ( byte )'|' )
        {
            return DefaultLoadResource( gamePath.Path, resourceManager, fileDescriptor, priority, isSync );
        }

        // Split the path into the special-treatment part (between the first and second '|')
        // and the actual path.
        byte ret   = 0;
        var  split = gamePath.Path.Split( ( byte )'|', 3, false );
        fileDescriptor->ResourceHandle->FileNameData   = split[ 2 ].Path;
        fileDescriptor->ResourceHandle->FileNameLength = split[ 2 ].Length;

        var funcFound = ResourceLoadCustomization.GetInvocationList()
           .Any( f => ( ( ResourceLoadCustomizationDelegate )f )
               .Invoke( split[ 1 ], split[ 2 ], resourceManager, fileDescriptor, priority, isSync, out ret ) );

        if( !funcFound )
        {
            ret = DefaultLoadResource( split[ 2 ], resourceManager, fileDescriptor, priority, isSync );
        }

        // Return original resource handle path so that they can be loaded separately.
        fileDescriptor->ResourceHandle->FileNameData   = gamePath.Path.Path;
        fileDescriptor->ResourceHandle->FileNameLength = gamePath.Path.Length;
        return ret;
    }

    // Load the resource from an SqPack and trigger the FileLoaded event.
    private byte DefaultResourceLoad( Utf8String path, ResourceManager* resourceManager,
        SeFileDescriptor* fileDescriptor, int priority, bool isSync )
    {
        var ret = Penumbra.ResourceLoader.ReadSqPackHook.Original( resourceManager, fileDescriptor, priority, isSync );
        FileLoaded?.Invoke( path, ret != 0, false );
        return ret;
    }

    // Load the resource from a path on the users hard drives.
    private byte DefaultRootedResourceLoad( Utf8String gamePath, ResourceManager* resourceManager,
        SeFileDescriptor* fileDescriptor, int priority, bool isSync )
    {
        // Specify that we are loading unpacked files from the drive.
        // We need to copy the actual file path in UTF16 (Windows-Unicode) on two locations,
        // but since we only allow ASCII in the game paths, this is just a matter of upcasting.
        fileDescriptor->FileMode = FileMode.LoadUnpackedResource;

        var fd = stackalloc byte[0x20 + 2 * gamePath.Length + 0x16];
        fileDescriptor->FileDescriptor = fd;
        var fdPtr = ( char* )( fd + 0x21 );
        for( var i = 0; i < gamePath.Length; ++i )
        {
            ( &fileDescriptor->Utf16FileName )[ i ] = ( char )gamePath.Path[ i ];
            fdPtr[ i ]                              = ( char )gamePath.Path[ i ];
        }

        ( &fileDescriptor->Utf16FileName )[ gamePath.Length ] = '\0';
        fdPtr[ gamePath.Length ]                              = '\0';

        // Use the SE ReadFile function.
        var ret = ReadFile( resourceManager, fileDescriptor, priority, isSync );
        FileLoaded?.Invoke( gamePath, ret != 0, true );
        return ret;
    }

    // Load a resource by its path. If it is rooted, it will be loaded from the drive, otherwise from the SqPack.
    internal byte DefaultLoadResource( Utf8String gamePath, ResourceManager* resourceManager, SeFileDescriptor* fileDescriptor, int priority,
        bool isSync )
        => Utf8GamePath.IsRooted( gamePath )
            ? DefaultRootedResourceLoad( gamePath, resourceManager, fileDescriptor, priority, isSync )
            : DefaultResourceLoad( gamePath, resourceManager, fileDescriptor, priority, isSync );

    private void DisposeHooks()
    {
        DisableHooks();
        ReadSqPackHook.Dispose();
        GetResourceSyncHook.Dispose();
        GetResourceAsyncHook.Dispose();
    }
}