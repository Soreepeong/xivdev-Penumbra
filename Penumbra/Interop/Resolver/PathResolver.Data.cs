using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Penumbra.Collections;
using Penumbra.GameData.ByteString;
using ObjectKind = Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace Penumbra.Interop.Resolver;

public unsafe partial class PathResolver
{
    // Keep track of created DrawObjects that are CharacterBase,
    // and use the last game object that called EnableDraw to link them.
    public delegate IntPtr CharacterBaseCreateDelegate( uint a, IntPtr b, IntPtr c, byte d );

    [Signature( "E8 ?? ?? ?? ?? 48 85 C0 74 21 C7 40", DetourName = "CharacterBaseCreateDetour" )]
    public Hook< CharacterBaseCreateDelegate >? CharacterBaseCreateHook;

    private ModCollection? _lastCreatedCollection;

    private IntPtr CharacterBaseCreateDetour( uint a, IntPtr b, IntPtr c, byte d )
    {
        using var cmp = MetaChanger.ChangeCmp( this, out _lastCreatedCollection );
        var       ret = CharacterBaseCreateHook!.Original( a, b, c, d );
        if( LastGameObject != null )
        {
            DrawObjectToObject[ ret ] = ( _lastCreatedCollection!, LastGameObject->ObjectIndex );
        }

        return ret;
    }


    // Remove DrawObjects from the list when they are destroyed.
    public delegate void CharacterBaseDestructorDelegate( IntPtr drawBase );

    [Signature( "E8 ?? ?? ?? ?? 40 F6 C7 01 74 3A 40 F6 C7 04 75 27 48 85 DB 74 2F 48 8B 05 ?? ?? ?? ?? 48 8B D3 48 8B 48 30",
        DetourName = "CharacterBaseDestructorDetour" )]
    public Hook< CharacterBaseDestructorDelegate >? CharacterBaseDestructorHook;

    private void CharacterBaseDestructorDetour( IntPtr drawBase )
    {
        DrawObjectToObject.Remove( drawBase );
        CharacterBaseDestructorHook!.Original.Invoke( drawBase );
    }


    // EnableDraw is what creates DrawObjects for gameObjects,
    // so we always keep track of the current GameObject to be able to link it to the DrawObject.
    public delegate void EnableDrawDelegate( IntPtr gameObject, IntPtr b, IntPtr c, IntPtr d );

    [Signature( "E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9 74 ?? 33 D2 E8 ?? ?? ?? ?? 84 C0" )]
    public Hook< EnableDrawDelegate >? EnableDrawHook;

    private void EnableDrawDetour( IntPtr gameObject, IntPtr b, IntPtr c, IntPtr d )
    {
        var oldObject = LastGameObject;
        LastGameObject = ( GameObject* )gameObject;
        EnableDrawHook!.Original.Invoke( gameObject, b, c, d );
        LastGameObject = oldObject;
    }

    // Not fully understood. The game object the weapon is loaded for is seemingly found at a1 + 8,
    // so we use that.
    public delegate void WeaponReloadFunc( IntPtr a1, uint a2, IntPtr a3, byte a4, byte a5, byte a6, byte a7 );

    [Signature( "E8 ?? ?? ?? ?? 44 8B 9F" )]
    public Hook< WeaponReloadFunc >? WeaponReloadHook;

    public void WeaponReloadDetour( IntPtr a1, uint a2, IntPtr a3, byte a4, byte a5, byte a6, byte a7 )
    {
        var oldGame = LastGameObject;
        LastGameObject = *( GameObject** )( a1 + 8 );
        WeaponReloadHook!.Original( a1, a2, a3, a4, a5, a6, a7 );
        LastGameObject = oldGame;
    }

    private void EnableDataHooks()
    {
        CharacterBaseCreateHook?.Enable();
        EnableDrawHook?.Enable();
        CharacterBaseDestructorHook?.Enable();
        WeaponReloadHook?.Enable();
        Penumbra.CollectionManager.CollectionChanged += CheckCollections;
        LoadTimelineResourcesHook?.Enable();
        CharacterBaseLoadAnimationHook?.Enable();
        LoadSomeAvfxHook?.Enable();
        LoadSomePapHook?.Enable();
        SomeActionLoadHook?.Enable();
    }

    private void DisableDataHooks()
    {
        Penumbra.CollectionManager.CollectionChanged -= CheckCollections;
        WeaponReloadHook?.Disable();
        CharacterBaseCreateHook?.Disable();
        EnableDrawHook?.Disable();
        CharacterBaseDestructorHook?.Disable();
        LoadTimelineResourcesHook?.Disable();
        CharacterBaseLoadAnimationHook?.Disable();
        LoadSomeAvfxHook?.Disable();
        LoadSomePapHook?.Disable();
        SomeActionLoadHook?.Disable();
    }

    private void DisposeDataHooks()
    {
        WeaponReloadHook?.Dispose();
        CharacterBaseCreateHook?.Dispose();
        EnableDrawHook?.Dispose();
        CharacterBaseDestructorHook?.Dispose();
        LoadTimelineResourcesHook?.Dispose();
        CharacterBaseLoadAnimationHook?.Dispose();
        LoadSomeAvfxHook?.Dispose();
        LoadSomePapHook?.Dispose();
        SomeActionLoadHook?.Dispose();
    }

    // This map links DrawObjects directly to Actors (by ObjectTable index) and their collections.
    // It contains any DrawObjects that correspond to a human actor, even those without specific collections.
    internal readonly Dictionary< IntPtr, (ModCollection, int) > DrawObjectToObject = new();

    // This map links files to their corresponding collection, if it is non-default.
    internal readonly ConcurrentDictionary< Utf8String, ModCollection > PathCollections = new();

    internal GameObject* LastGameObject = null;

    // Check that a linked DrawObject still corresponds to the correct actor and that it still exists, otherwise remove it.
    private bool VerifyEntry( IntPtr drawObject, int gameObjectIdx, out GameObject* gameObject )
    {
        var tmp = Dalamud.Objects[ gameObjectIdx ];
        if( tmp != null )
        {
            gameObject = ( GameObject* )tmp.Address;
            if( gameObject->DrawObject == ( DrawObject* )drawObject )
            {
                return true;
            }
        }

        gameObject = null;
        DrawObjectToObject.Remove( drawObject );
        return false;
    }

    // Obtain the name of the current player, if one exists.
    private static string? GetPlayerName()
        => Dalamud.Objects[ 0 ]?.Name.ToString();

    // Obtain the name of the inspect target from its window, if it exists.
    private static string? GetInspectName()
    {
        if( !Penumbra.Config.UseCharacterCollectionInInspect )
        {
            return null;
        }

        var addon = Dalamud.GameGui.GetAddonByName( "CharacterInspect", 1 );
        if( addon == IntPtr.Zero )
        {
            return null;
        }

        var ui = ( AtkUnitBase* )addon;
        if( ui->UldManager.NodeListCount < 60 )
        {
            return null;
        }

        var text = ( AtkTextNode* )ui->UldManager.NodeList[ 59 ];
        if( text == null || !text->AtkResNode.IsVisible )
        {
            text = ( AtkTextNode* )ui->UldManager.NodeList[ 60 ];
        }

        return text != null ? text->NodeText.ToString() : null;
    }

    // Obtain the name displayed in the Character Card from the agent.
    private static string? GetCardName()
    {
        if( !Penumbra.Config.UseCharacterCollectionsInCards )
        {
            return null;
        }

        var uiModule    = ( UIModule* )Dalamud.GameGui.GetUIModule();
        var agentModule = uiModule->GetAgentModule();
        var agent       = ( byte* )agentModule->GetAgentByInternalID( 393 );
        if( agent == null )
        {
            return null;
        }

        var data = *( byte** )( agent + 0x28 );
        if( data == null )
        {
            return null;
        }

        var block = data + 0x7A;
        return new Utf8String( block ).ToString();
    }

    // Obtain the name of the player character if the glamour plate edit window is open.
    private static string? GetGlamourName()
    {
        if( !Penumbra.Config.UseCharacterCollectionInTryOn )
        {
            return null;
        }

        var addon = Dalamud.GameGui.GetAddonByName( "MiragePrismMiragePlate", 1 );
        return addon == IntPtr.Zero ? null : GetPlayerName();
    }

    // Guesstimate whether an unnamed cutscene actor corresponds to the player or not,
    // and if so, return the player name.
    private static string? GetCutsceneName( GameObject* gameObject )
    {
        if( gameObject->Name[ 0 ] != 0 || gameObject->ObjectKind != ( byte )ObjectKind.Player )
        {
            return null;
        }

        var player = Dalamud.Objects[ 0 ];
        if( player == null )
        {
            return null;
        }

        var pc = ( Character* )player.Address;
        return pc->ClassJob == ( ( Character* )gameObject )->ClassJob ? player.Name.ToString() : null;
    }

    // Identify the owner of a companion, mount or monster and apply the corresponding collection.
    // Companions and mounts get set to the actor before them in the table if it exists.
    // Monsters with a owner use that owner if it exists.
    private static string? GetOwnerName( GameObject* gameObject )
    {
        if( !Penumbra.Config.UseOwnerNameForCharacterCollection )
        {
            return null;
        }

        GameObject* owner = null;
        if( ( ObjectKind )gameObject->GetObjectKind() is ObjectKind.Companion or ObjectKind.MountType && gameObject->ObjectIndex > 0 )
        {
            owner = ( GameObject* )Dalamud.Objects[ gameObject->ObjectIndex - 1 ]?.Address;
        }
        else if( gameObject->OwnerID != 0xE0000000 )
        {
            owner = ( GameObject* )( Dalamud.Objects.SearchById( gameObject->OwnerID )?.Address ?? IntPtr.Zero );
        }

        if( owner != null )
        {
            return new Utf8String( owner->Name ).ToString();
        }

        return null;
    }

    // Identify the correct collection for a GameObject by index and name.
    private static ModCollection IdentifyCollection( GameObject* gameObject )
    {
        if( gameObject == null )
        {
            return Penumbra.CollectionManager.Default;
        }

        // Housing Retainers
        if( Penumbra.Config.UseDefaultCollectionForRetainers
        && gameObject->ObjectKind == ( byte )ObjectKind.EventNpc
        && gameObject->DataID     == 1011832 )
        {
            return Penumbra.CollectionManager.Default;
        }

        string? actorName = null;
        if( Penumbra.Config.PreferNamedCollectionsOverOwners )
        {
            // Early return if we prefer the actors own name over its owner.
            actorName = new Utf8String( gameObject->Name ).ToString();
            if( actorName.Length > 0 && Penumbra.CollectionManager.Characters.TryGetValue( actorName, out var actorCollection ) )
            {
                return actorCollection;
            }
        }

        // All these special cases are relevant for an empty name, so never collide with the above setting.
        // Only OwnerName can be applied to something with a non-empty name, and that is the specific case we want to handle.
        var actualName = gameObject->ObjectIndex switch
            {
                240    => Penumbra.Config.UseCharacterCollectionInMainWindow ? GetPlayerName() : null, // character window
                241    => GetInspectName() ?? GetCardName() ?? GetGlamourName(), // inspect, character card, glamour plate editor.
                242    => Penumbra.Config.UseCharacterCollectionInTryOn ? GetPlayerName() : null, // try-on
                243    => Penumbra.Config.UseCharacterCollectionInTryOn ? GetPlayerName() : null, // dye preview
                >= 200 => GetCutsceneName( gameObject ),
                _      => null,
            }
         ?? GetOwnerName( gameObject ) ?? actorName ?? new Utf8String( gameObject->Name ).ToString();

        // First check temporary character collections, then the own configuration.
        return Penumbra.TempMods.Collections.TryGetValue(actualName, out var c) ? c : Penumbra.CollectionManager.Character( actualName );
    }

    // Update collections linked to Game/DrawObjects due to a change in collection configuration.
    private void CheckCollections( ModCollection.Type type, ModCollection? _1, ModCollection? _2, string? name )
    {
        if( type is not (ModCollection.Type.Character or ModCollection.Type.Default) )
        {
            return;
        }

        foreach( var (key, (_, idx)) in DrawObjectToObject.ToArray() )
        {
            if( !VerifyEntry( key, idx, out var obj ) )
            {
                DrawObjectToObject.Remove( key );
            }

            var newCollection = IdentifyCollection( obj );
            DrawObjectToObject[ key ] = ( newCollection, idx );
        }
    }

    // Use the stored information to find the GameObject and Collection linked to a DrawObject.
    private GameObject* FindParent( IntPtr drawObject, out ModCollection collection )
    {
        if( DrawObjectToObject.TryGetValue( drawObject, out var data ) )
        {
            var gameObjectIdx = data.Item2;
            if( VerifyEntry( drawObject, gameObjectIdx, out var gameObject ) )
            {
                collection = data.Item1;
                return gameObject;
            }
        }

        if( LastGameObject != null && ( LastGameObject->DrawObject == null || LastGameObject->DrawObject == ( DrawObject* )drawObject ) )
        {
            collection = IdentifyCollection( LastGameObject );
            return LastGameObject;
        }


        collection = IdentifyCollection( null );
        return null;
    }


    // Special handling for paths so that we do not store non-owned temporary strings in the dictionary.
    private void SetCollection( Utf8String path, ModCollection collection )
    {
        if( PathCollections.ContainsKey( path ) || path.IsOwned )
        {
            PathCollections[ path ] = collection;
        }
        else
        {
            PathCollections[ path.Clone() ] = collection;
        }
    }

    // Find all current DrawObjects used in the GameObject table.
    private void InitializeDrawObjects()
    {
        foreach( var gameObject in Dalamud.Objects )
        {
            var ptr = ( GameObject* )gameObject.Address;
            if( ptr->IsCharacter() && ptr->DrawObject != null )
            {
                DrawObjectToObject[ ( IntPtr )ptr->DrawObject ] = ( IdentifyCollection( ptr ), ptr->ObjectIndex );
            }
        }
    }
}