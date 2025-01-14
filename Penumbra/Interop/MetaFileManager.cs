using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using Penumbra.GameData.ByteString;
using Penumbra.Interop.Structs;

namespace Penumbra.Interop;

public unsafe class MetaFileManager : IDisposable
{
    public MetaFileManager()
    {
        SignatureHelper.Initialise( this );
        InitImc();
    }

    public void Dispose()
    {
        DisposeImc();
    }


    // Allocate in the games space for file storage.
    // We only need this if using any meta file.
#if USE_IMC || USE_CMP || USE_EQDP || USE_EQP || USE_EST || USE_GMP
    [Signature( "E8 ?? ?? ?? ?? 41 B9 ?? ?? ?? ?? 4C 8B C0" )]
    public IntPtr GetFileSpaceAddress;
#endif
    public IMemorySpace* GetFileSpace()
        => ( ( delegate* unmanaged< IMemorySpace* > )GetFileSpaceAddress )();

    public void* AllocateFileMemory( ulong length, ulong alignment = 0 )
        => GetFileSpace()->Malloc( length, alignment );

    public void* AllocateFileMemory( int length, int alignment = 0 )
        => AllocateFileMemory( ( ulong )length, ( ulong )alignment );


    // We only need this for IMC files, since we need to hook their cleanup function.
#if USE_IMC
    [Signature( "48 8D 05 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 89 03", ScanType = ScanType.StaticAddress )]
    public IntPtr* DefaultResourceHandleVTable;
#endif

    public delegate void                  ClearResource( ResourceHandle* resource );
    public          Hook< ClearResource > ClearDefaultResourceHook = null!;

    private readonly Dictionary< IntPtr, (IntPtr, int) > _originalImcData = new();

    // We store the original data of loaded IMCs so that we can restore it before they get destroyed,
    // similar to the other meta files, just with arbitrary destruction.
    private void ClearDefaultResourceDetour( ResourceHandle* resource )
    {
        if( _originalImcData.TryGetValue( ( IntPtr )resource, out var data ) )
        {
            PluginLog.Debug( "Restoring data of {$Name:l} (0x{Resource}) to 0x{Data:X} and Length {Length} before deletion.",
                Utf8String.FromSpanUnsafe( resource->FileNameSpan(), true, null, null ), ( ulong )resource, ( ulong )data.Item1, data.Item2 );
            resource->SetData( data.Item1, data.Item2 );
            _originalImcData.Remove( ( IntPtr )resource );
        }

        ClearDefaultResourceHook.Original( resource );
    }

    // Called when a new IMC is manipulated to store its data.
    [Conditional( "USE_IMC" )]
    public void AddImcFile( ResourceHandle* resource, IntPtr data, int length )
    {
        PluginLog.Debug( "Storing data 0x{Data:X} of Length {Length} for {$Name:l} (0x{Resource:X}).", ( ulong )data, length,
            Utf8String.FromSpanUnsafe( resource->FileNameSpan(), true, null, null ), ( ulong )resource );
        _originalImcData[ ( IntPtr )resource ] = ( data, length );
    }

    // Initialize the hook at VFunc 25, which is called when default resources (and IMC resources do not overwrite it) destroy their data.
    [Conditional( "USE_IMC" )]
    private void InitImc()
    {
        ClearDefaultResourceHook = new Hook< ClearResource >( DefaultResourceHandleVTable[ 25 ], ClearDefaultResourceDetour );
        ClearDefaultResourceHook.Enable();
    }

    [Conditional( "USE_IMC" )]
    private void DisposeImc()
    {
        ClearDefaultResourceHook.Disable();
        ClearDefaultResourceHook.Dispose();
        // Restore all IMCs to their default values on dispose.
        // This should only be relevant when testing/disabling/reenabling penumbra.
        foreach( var (resourcePtr, (data, length)) in _originalImcData )
        {
            var resource = ( ResourceHandle* )resourcePtr;
            resource->SetData( data, length );
        }

        _originalImcData.Clear();
    }
}