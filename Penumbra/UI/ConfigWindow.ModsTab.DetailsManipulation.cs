//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Numerics;
//using Dalamud.Interface;
//using ImGuiNET;
//using Penumbra.GameData.Enums;
//using Penumbra.GameData.Structs;
//using Penumbra.Meta.Files;
//using Penumbra.Meta.Manipulations;
//using Penumbra.UI.Custom;
//using ObjectType = Penumbra.GameData.Enums.ObjectType;
//
//namespace Penumbra.UI;
//
//public partial class SettingsInterface
//{
//    private partial class PluginDetails
//    {
//        private int    _newManipTypeIdx     = 0;
//        private ushort _newManipSetId       = 0;
//        private ushort _newManipSecondaryId = 0;
//        private int    _newManipSubrace     = 0;
//        private int    _newManipRace        = 0;
//        private int    _newManipAttribute   = 0;
//        private int    _newManipEquipSlot   = 0;
//        private int    _newManipObjectType  = 0;
//        private int    _newManipGender      = 0;
//        private int    _newManipBodySlot    = 0;
//        private ushort _newManipVariant     = 0;
//
//
//        private static readonly (string, EquipSlot)[] EqpEquipSlots =
//        {
//            ( "Head", EquipSlot.Head ),
//            ( "Body", EquipSlot.Body ),
//            ( "Hands", EquipSlot.Hands ),
//            ( "Legs", EquipSlot.Legs ),
//            ( "Feet", EquipSlot.Feet ),
//        };
//
//        private static readonly (string, EquipSlot)[] EqdpEquipSlots =
//        {
//            EqpEquipSlots[ 0 ],
//            EqpEquipSlots[ 1 ],
//            EqpEquipSlots[ 2 ],
//            EqpEquipSlots[ 3 ],
//            EqpEquipSlots[ 4 ],
//            ( "Ears", EquipSlot.Ears ),
//            ( "Neck", EquipSlot.Neck ),
//            ( "Wrist", EquipSlot.Wrists ),
//            ( "Left Finger", EquipSlot.LFinger ),
//            ( "Right Finger", EquipSlot.RFinger ),
//        };
//
//        private static readonly (string, ModelRace)[] Races =
//        {
//            ( ModelRace.Midlander.ToName(), ModelRace.Midlander ),
//            ( ModelRace.Highlander.ToName(), ModelRace.Highlander ),
//            ( ModelRace.Elezen.ToName(), ModelRace.Elezen ),
//            ( ModelRace.Miqote.ToName(), ModelRace.Miqote ),
//            ( ModelRace.Roegadyn.ToName(), ModelRace.Roegadyn ),
//            ( ModelRace.Lalafell.ToName(), ModelRace.Lalafell ),
//            ( ModelRace.AuRa.ToName(), ModelRace.AuRa ),
//            ( ModelRace.Viera.ToName(), ModelRace.Viera ),
//            ( ModelRace.Hrothgar.ToName(), ModelRace.Hrothgar ),
//        };
//
//        private static readonly (string, Gender)[] Genders =
//        {
//            ( Gender.Male.ToName(), Gender.Male ),
//            ( Gender.Female.ToName(), Gender.Female ),
//            ( Gender.MaleNpc.ToName(), Gender.MaleNpc ),
//            ( Gender.FemaleNpc.ToName(), Gender.FemaleNpc ),
//        };
//
//        private static readonly (string, EstManipulation.EstType)[] EstTypes =
//        {
//            ( "Hair", EstManipulation.EstType.Hair ),
//            ( "Face", EstManipulation.EstType.Face ),
//            ( "Body", EstManipulation.EstType.Body ),
//            ( "Head", EstManipulation.EstType.Head ),
//        };
//
//        private static readonly (string, SubRace)[] Subraces =
//        {
//            ( SubRace.Midlander.ToName(), SubRace.Midlander ),
//            ( SubRace.Highlander.ToName(), SubRace.Highlander ),
//            ( SubRace.Wildwood.ToName(), SubRace.Wildwood ),
//            ( SubRace.Duskwight.ToName(), SubRace.Duskwight ),
//            ( SubRace.SeekerOfTheSun.ToName(), SubRace.SeekerOfTheSun ),
//            ( SubRace.KeeperOfTheMoon.ToName(), SubRace.KeeperOfTheMoon ),
//            ( SubRace.Seawolf.ToName(), SubRace.Seawolf ),
//            ( SubRace.Hellsguard.ToName(), SubRace.Hellsguard ),
//            ( SubRace.Plainsfolk.ToName(), SubRace.Plainsfolk ),
//            ( SubRace.Dunesfolk.ToName(), SubRace.Dunesfolk ),
//            ( SubRace.Raen.ToName(), SubRace.Raen ),
//            ( SubRace.Xaela.ToName(), SubRace.Xaela ),
//            ( SubRace.Rava.ToName(), SubRace.Rava ),
//            ( SubRace.Veena.ToName(), SubRace.Veena ),
//            ( SubRace.Helion.ToName(), SubRace.Helion ),
//            ( SubRace.Lost.ToName(), SubRace.Lost ),
//        };
//
//        private static readonly (string, RspAttribute)[] RspAttributes =
//        {
//            ( RspAttribute.MaleMinSize.ToFullString(), RspAttribute.MaleMinSize ),
//            ( RspAttribute.MaleMaxSize.ToFullString(), RspAttribute.MaleMaxSize ),
//            ( RspAttribute.FemaleMinSize.ToFullString(), RspAttribute.FemaleMinSize ),
//            ( RspAttribute.FemaleMaxSize.ToFullString(), RspAttribute.FemaleMaxSize ),
//            ( RspAttribute.BustMinX.ToFullString(), RspAttribute.BustMinX ),
//            ( RspAttribute.BustMaxX.ToFullString(), RspAttribute.BustMaxX ),
//            ( RspAttribute.BustMinY.ToFullString(), RspAttribute.BustMinY ),
//            ( RspAttribute.BustMaxY.ToFullString(), RspAttribute.BustMaxY ),
//            ( RspAttribute.BustMinZ.ToFullString(), RspAttribute.BustMinZ ),
//            ( RspAttribute.BustMaxZ.ToFullString(), RspAttribute.BustMaxZ ),
//            ( RspAttribute.MaleMinTail.ToFullString(), RspAttribute.MaleMinTail ),
//            ( RspAttribute.MaleMaxTail.ToFullString(), RspAttribute.MaleMaxTail ),
//            ( RspAttribute.FemaleMinTail.ToFullString(), RspAttribute.FemaleMinTail ),
//            ( RspAttribute.FemaleMaxTail.ToFullString(), RspAttribute.FemaleMaxTail ),
//        };
//
//
//        private static readonly (string, ObjectType)[] ImcObjectType =
//        {
//            ( "Equipment", ObjectType.Equipment ),
//            ( "Customization", ObjectType.Character ),
//            ( "Weapon", ObjectType.Weapon ),
//            ( "Demihuman", ObjectType.DemiHuman ),
//            ( "Monster", ObjectType.Monster ),
//        };
//
//        private static readonly (string, BodySlot)[] ImcBodySlots =
//        {
//            ( "Hair", BodySlot.Hair ),
//            ( "Face", BodySlot.Face ),
//            ( "Body", BodySlot.Body ),
//            ( "Tail", BodySlot.Tail ),
//            ( "Ears", BodySlot.Zear ),
//        };
//
//        private static bool PrintCheckBox( string name, ref bool value, bool def )
//        {
//            var color = value == def ? 0 : value ? ColorDarkGreen : ColorDarkRed;
//            if( color == 0 )
//            {
//                return ImGui.Checkbox( name, ref value );
//            }
//
//            using var colorRaii = ImGuiRaii.PushColor( ImGuiCol.Text, color );
//            var       ret       = ImGui.Checkbox( name, ref value );
//            return ret;
//        }
//
//        private bool RestrictedInputInt( string name, ref ushort value, ushort min, ushort max )
//        {
//            int tmp = value;
//            if( ImGui.InputInt( name, ref tmp, 0, 0, _editMode ? ImGuiInputTextFlags.EnterReturnsTrue : ImGuiInputTextFlags.ReadOnly )
//            && tmp != value
//            && tmp >= min
//            && tmp <= max )
//            {
//                value = ( ushort )tmp;
//                return true;
//            }
//
//            return false;
//        }
//
//        private static bool DefaultButton< T >( string name, ref T value, T defaultValue ) where T : IComparable< T >
//        {
//            var compare = defaultValue.CompareTo( value );
//            var color = compare < 0 ? ColorDarkGreen :
//                compare         > 0 ? ColorDarkRed : ImGui.ColorConvertFloat4ToU32( ImGui.GetStyle().Colors[ ( int )ImGuiCol.Button ] );
//
//            using var colorRaii = ImGuiRaii.PushColor( ImGuiCol.Button, color );
//            var       ret       = ImGui.Button( name, Vector2.UnitX * 120 ) && compare != 0;
//            ImGui.SameLine();
//            return ret;
//        }
//
//        private bool DrawInputWithDefault( string name, ref ushort value, ushort defaultValue, ushort max )
//            => DefaultButton( $"{( _editMode ? "Set to " : "" )}Default: {defaultValue}##imc{name}", ref value, defaultValue )
//             || RestrictedInputInt( name, ref value, 0, max );
//
//        private static bool CustomCombo< T >( string label, IList< (string, T) > namesAndValues, out T value, ref int idx )
//        {
//            value = idx < namesAndValues.Count ? namesAndValues[ idx ].Item2 : default!;
//
//            if( !ImGui.BeginCombo( label, idx < namesAndValues.Count ? namesAndValues[ idx ].Item1 : string.Empty ) )
//            {
//                return false;
//            }
//
//            using var raii = ImGuiRaii.DeferredEnd( ImGui.EndCombo );
//
//            for( var i = 0; i < namesAndValues.Count; ++i )
//            {
//                if( !ImGui.Selectable( $"{namesAndValues[ i ].Item1}##{label}{i}", idx == i ) || idx == i )
//                {
//                    continue;
//                }
//
//                idx   = i;
//                value = namesAndValues[ i ].Item2;
//                return true;
//            }
//
//            return false;
//        }
//
//        private bool DrawEqpRow( int manipIdx, IList< MetaManipulation > list )
//        {
//            var ret = false;
//            var id  = list[ manipIdx ].Eqp;
//            var val = id.Entry;
//
//
//            if( ImGui.BeginPopup( $"##MetaPopup{manipIdx}" ) )
//            {
//                using var raii       = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//                var       defaults   = ExpandedEqpFile.GetDefault( id.SetId );
//                var       attributes = Eqp.EqpAttributes[ id.Slot ];
//
//                foreach( var flag in attributes )
//                {
//                    var name = flag.ToLocalName();
//                    var tmp  = val.HasFlag( flag );
//                    if( PrintCheckBox( $"{name}##manip", ref tmp, defaults.HasFlag( flag ) ) && _editMode && tmp != val.HasFlag( flag ) )
//                    {
//                        list[ manipIdx ] = new MetaManipulation( new EqpManipulation( tmp ? val | flag : val & ~flag, id.Slot, id.SetId ) );
//                        ret              = true;
//                    }
//                }
//            }
//
//            ImGui.Text( ObjectType.Equipment.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.SetId.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Slot.ToString() );
//            return ret;
//        }
//
//        private bool DrawGmpRow( int manipIdx, IList< MetaManipulation > list )
//        {
//            var ret = false;
//            var id  = list[ manipIdx ].Gmp;
//            var val = id.Entry;
//
//            if( ImGui.BeginPopup( $"##MetaPopup{manipIdx}" ) )
//            {
//                using var raii      = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//                var       defaults  = ExpandedGmpFile.GetDefault( id.SetId );
//                var       enabled   = val.Enabled;
//                var       animated  = val.Animated;
//                var       rotationA = val.RotationA;
//                var       rotationB = val.RotationB;
//                var       rotationC = val.RotationC;
//                ushort    unk       = val.UnknownTotal;
//
//                ret |= PrintCheckBox( "Visor Enabled##manip", ref enabled, defaults.Enabled ) && enabled != val.Enabled;
//                ret |= PrintCheckBox( "Visor Animated##manip", ref animated, defaults.Animated );
//                ret |= DrawInputWithDefault( "Rotation A##manip", ref rotationA, defaults.RotationA, 0x3FF );
//                ret |= DrawInputWithDefault( "Rotation B##manip", ref rotationB, defaults.RotationB, 0x3FF );
//                ret |= DrawInputWithDefault( "Rotation C##manip", ref rotationC, defaults.RotationC, 0x3FF );
//                ret |= DrawInputWithDefault( "Unknown Byte##manip", ref unk, defaults.UnknownTotal, 0xFF );
//
//                if( ret && _editMode )
//                {
//                    list[ manipIdx ] = new MetaManipulation( new GmpManipulation( new GmpEntry
//                    {
//                        Animated  = animated, Enabled    = enabled, UnknownTotal = ( byte )unk,
//                        RotationA = rotationA, RotationB = rotationB, RotationC  = rotationC,
//                    }, id.SetId ) );
//                }
//            }
//
//            ImGui.Text( ObjectType.Equipment.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.SetId.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( EquipSlot.Head.ToString() );
//            return ret;
//        }
//
//        private static (bool, bool) GetEqdpBits( EquipSlot slot, EqdpEntry entry )
//        {
//            return slot switch
//            {
//                EquipSlot.Head    => ( entry.HasFlag( EqdpEntry.Head1 ), entry.HasFlag( EqdpEntry.Head2 ) ),
//                EquipSlot.Body    => ( entry.HasFlag( EqdpEntry.Body1 ), entry.HasFlag( EqdpEntry.Body2 ) ),
//                EquipSlot.Hands   => ( entry.HasFlag( EqdpEntry.Hands1 ), entry.HasFlag( EqdpEntry.Hands2 ) ),
//                EquipSlot.Legs    => ( entry.HasFlag( EqdpEntry.Legs1 ), entry.HasFlag( EqdpEntry.Legs2 ) ),
//                EquipSlot.Feet    => ( entry.HasFlag( EqdpEntry.Feet1 ), entry.HasFlag( EqdpEntry.Feet2 ) ),
//                EquipSlot.Neck    => ( entry.HasFlag( EqdpEntry.Neck1 ), entry.HasFlag( EqdpEntry.Neck2 ) ),
//                EquipSlot.Ears    => ( entry.HasFlag( EqdpEntry.Ears1 ), entry.HasFlag( EqdpEntry.Ears2 ) ),
//                EquipSlot.Wrists  => ( entry.HasFlag( EqdpEntry.Wrists1 ), entry.HasFlag( EqdpEntry.Wrists2 ) ),
//                EquipSlot.RFinger => ( entry.HasFlag( EqdpEntry.RingR1 ), entry.HasFlag( EqdpEntry.RingR2 ) ),
//                EquipSlot.LFinger => ( entry.HasFlag( EqdpEntry.RingL1 ), entry.HasFlag( EqdpEntry.RingL2 ) ),
//                _                 => ( false, false ),
//            };
//        }
//
//        private static EqdpEntry SetEqdpBits( EquipSlot slot, EqdpEntry value, bool bit1, bool bit2 )
//        {
//            switch( slot )
//            {
//                case EquipSlot.Head:
//                    value = bit1 ? value | EqdpEntry.Head1 : value & ~EqdpEntry.Head1;
//                    value = bit2 ? value | EqdpEntry.Head2 : value & ~EqdpEntry.Head2;
//                    return value;
//                case EquipSlot.Body:
//                    value = bit1 ? value | EqdpEntry.Body1 : value & ~EqdpEntry.Body1;
//                    value = bit2 ? value | EqdpEntry.Body2 : value & ~EqdpEntry.Body2;
//                    return value;
//                case EquipSlot.Hands:
//                    value = bit1 ? value | EqdpEntry.Hands1 : value & ~EqdpEntry.Hands1;
//                    value = bit2 ? value | EqdpEntry.Hands2 : value & ~EqdpEntry.Hands2;
//                    return value;
//                case EquipSlot.Legs:
//                    value = bit1 ? value | EqdpEntry.Legs1 : value & ~EqdpEntry.Legs1;
//                    value = bit2 ? value | EqdpEntry.Legs2 : value & ~EqdpEntry.Legs2;
//                    return value;
//                case EquipSlot.Feet:
//                    value = bit1 ? value | EqdpEntry.Feet1 : value & ~EqdpEntry.Feet1;
//                    value = bit2 ? value | EqdpEntry.Feet2 : value & ~EqdpEntry.Feet2;
//                    return value;
//                case EquipSlot.Neck:
//                    value = bit1 ? value | EqdpEntry.Neck1 : value & ~EqdpEntry.Neck1;
//                    value = bit2 ? value | EqdpEntry.Neck2 : value & ~EqdpEntry.Neck2;
//                    return value;
//                case EquipSlot.Ears:
//                    value = bit1 ? value | EqdpEntry.Ears1 : value & ~EqdpEntry.Ears1;
//                    value = bit2 ? value | EqdpEntry.Ears2 : value & ~EqdpEntry.Ears2;
//                    return value;
//                case EquipSlot.Wrists:
//                    value = bit1 ? value | EqdpEntry.Wrists1 : value & ~EqdpEntry.Wrists1;
//                    value = bit2 ? value | EqdpEntry.Wrists2 : value & ~EqdpEntry.Wrists2;
//                    return value;
//                case EquipSlot.RFinger:
//                    value = bit1 ? value | EqdpEntry.RingR1 : value & ~EqdpEntry.RingR1;
//                    value = bit2 ? value | EqdpEntry.RingR2 : value & ~EqdpEntry.RingR2;
//                    return value;
//                case EquipSlot.LFinger:
//                    value = bit1 ? value | EqdpEntry.RingL1 : value & ~EqdpEntry.RingL1;
//                    value = bit2 ? value | EqdpEntry.RingL2 : value & ~EqdpEntry.RingL2;
//                    return value;
//            }
//
//            return value;
//        }
//
//        private bool DrawEqdpRow( int manipIdx, IList< MetaManipulation > list )
//        {
//            var ret = false;
//            var id  = list[ manipIdx ].Eqdp;
//            var val = id.Entry;
//
//            if( ImGui.BeginPopup( $"##MetaPopup{manipIdx}" ) )
//            {
//                using var raii     = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//                var       defaults = ExpandedEqdpFile.GetDefault( id.FileIndex(), id.SetId );
//                var (bit1, bit2)       = GetEqdpBits( id.Slot, val );
//                var (defBit1, defBit2) = GetEqdpBits( id.Slot, defaults );
//
//                ret |= PrintCheckBox( "Bit 1##manip", ref bit1, defBit1 );
//                ret |= PrintCheckBox( "Bit 2##manip", ref bit2, defBit2 );
//
//                if( ret && _editMode )
//                {
//                    list[ manipIdx ] = new MetaManipulation( new EqdpManipulation( SetEqdpBits( id.Slot, val, bit1, bit2 ), id.Slot, id.Gender,
//                        id.Race, id.SetId ) );
//                }
//            }
//
//            ImGui.Text( id.Slot.IsAccessory()
//                ? ObjectType.Accessory.ToString()
//                : ObjectType.Equipment.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.SetId.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Slot.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Race.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Gender.ToString() );
//            return ret;
//        }
//
//        private bool DrawEstRow( int manipIdx, IList< MetaManipulation > list )
//        {
//            var ret = false;
//            var id  = list[ manipIdx ].Est;
//            var val = id.Entry;
//
//            if( ImGui.BeginPopup( $"##MetaPopup{manipIdx}" ) )
//            {
//                using var raii     = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//                var       defaults = EstFile.GetDefault( id.Slot, Names.CombinedRace( id.Gender, id.Race ), id.SetId );
//                if( DrawInputWithDefault( "No Idea what this does!##manip", ref val, defaults, ushort.MaxValue ) && _editMode )
//                {
//                    list[ manipIdx ] = new MetaManipulation( new EstManipulation( id.Gender, id.Race, id.Slot, id.SetId, val ) );
//                    ret              = true;
//                }
//            }
//
//            ImGui.Text( id.Slot.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.SetId.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Race.ToName() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Gender.ToName() );
//
//            return ret;
//        }
//
//        private bool DrawImcRow( int manipIdx, IList< MetaManipulation > list )
//        {
//            var ret = false;
//            var id  = list[ manipIdx ].Imc;
//            var val = id.Entry;
//
//            if( ImGui.BeginPopup( $"##MetaPopup{manipIdx}" ) )
//            {
//                using var raii                = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//                var       defaults            = new ImcFile( id.GamePath() ).GetEntry( ImcFile.PartIndex( id.EquipSlot ), id.Variant );
//                ushort    materialId          = val.MaterialId;
//                ushort    vfxId               = val.VfxId;
//                ushort    decalId             = val.DecalId;
//                var       soundId             = ( ushort )val.SoundId;
//                var       attributeMask       = val.AttributeMask;
//                var       materialAnimationId = ( ushort )val.MaterialAnimationId;
//                ret |= DrawInputWithDefault( "Material Id", ref materialId, defaults.MaterialId, byte.MaxValue );
//                ret |= DrawInputWithDefault( "Vfx Id", ref vfxId, defaults.VfxId, byte.MaxValue );
//                ret |= DrawInputWithDefault( "Decal Id", ref decalId, defaults.DecalId, byte.MaxValue );
//                ret |= DrawInputWithDefault( "Sound Id", ref soundId, defaults.SoundId, 0x3F );
//                ret |= DrawInputWithDefault( "Attribute Mask", ref attributeMask, defaults.AttributeMask, 0x3FF );
//                ret |= DrawInputWithDefault( "Material Animation Id", ref materialAnimationId, defaults.MaterialAnimationId,
//                    byte.MaxValue );
//
//                if( ret && _editMode )
//                {
//                    var value = new ImcEntry( ( byte )materialId, ( byte )decalId, attributeMask, ( byte )soundId, ( byte )vfxId,
//                        ( byte )materialAnimationId );
//                    list[ manipIdx ] = new MetaManipulation( new ImcManipulation( id, value ) );
//                }
//            }
//
//            ImGui.Text( id.ObjectType.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.PrimaryId.ToString() );
//            ImGui.TableNextColumn();
//            if( id.ObjectType is ObjectType.Accessory or ObjectType.Equipment )
//            {
//                ImGui.Text( id.ObjectType is ObjectType.Equipment or ObjectType.Accessory
//                    ? id.EquipSlot.ToString()
//                    : id.BodySlot.ToString() );
//            }
//
//            ImGui.TableNextColumn();
//            ImGui.TableNextColumn();
//            ImGui.TableNextColumn();
//            if( id.ObjectType != ObjectType.Equipment
//            && id.ObjectType  != ObjectType.Accessory )
//            {
//                ImGui.Text( id.SecondaryId.ToString() );
//            }
//
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Variant.ToString() );
//            return ret;
//        }
//
//        private bool DrawRspRow( int manipIdx, IList< MetaManipulation > list )
//        {
//            var ret      = false;
//            var id       = list[ manipIdx ].Rsp;
//            var defaults = CmpFile.GetDefault( id.SubRace, id.Attribute );
//            var val      = id.Entry;
//            if( ImGui.BeginPopup( $"##MetaPopup{manipIdx}" ) )
//            {
//                using var raii = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//                if( DefaultButton(
//                       $"{( _editMode ? "Set to " : "" )}Default: {defaults:F3}##scaleManip", ref val, defaults )
//                && _editMode )
//                {
//                    list[ manipIdx ] = new MetaManipulation( new RspManipulation( id.SubRace, id.Attribute, val ) );
//                    ret              = true;
//                }
//
//                ImGui.SetNextItemWidth( 50 * ImGuiHelpers.GlobalScale );
//                if( ImGui.InputFloat( "Scale###manip", ref val, 0, 0, "%.3f",
//                       _editMode ? ImGuiInputTextFlags.EnterReturnsTrue : ImGuiInputTextFlags.ReadOnly )
//                && val >= 0
//                && val <= 5
//                && _editMode )
//                {
//                    list[ manipIdx ] = new MetaManipulation( new RspManipulation( id.SubRace, id.Attribute, val ) );
//                    ret              = true;
//                }
//            }
//
//            ImGui.Text( id.Attribute.ToUngenderedString() );
//            ImGui.TableNextColumn();
//            ImGui.TableNextColumn();
//            ImGui.TableNextColumn();
//            ImGui.Text( id.SubRace.ToString() );
//            ImGui.TableNextColumn();
//            ImGui.Text( id.Attribute.ToGender().ToString() );
//            return ret;
//        }
//
//        private bool DrawManipulationRow( ref int manipIdx, IList< MetaManipulation > list, ref int count )
//        {
//            var type = list[ manipIdx ].ManipulationType;
//
//            if( _editMode )
//            {
//                ImGui.TableNextColumn();
//                using var font = ImGuiRaii.PushFont( UiBuilder.IconFont );
//                if( ImGui.Button( $"{FontAwesomeIcon.Trash.ToIconString()}##manipDelete{manipIdx}" ) )
//                {
//                    list.RemoveAt( manipIdx );
//                    ImGui.TableNextRow();
//                    --manipIdx;
//                    --count;
//                    return true;
//                }
//            }
//
//            ImGui.TableNextColumn();
//            ImGui.Text( type.ToString() );
//            ImGui.TableNextColumn();
//
//            var changes = false;
//            switch( type )
//            {
//                case MetaManipulation.Type.Eqp:
//                    changes = DrawEqpRow( manipIdx, list );
//                    ImGui.TableSetColumnIndex( 9 );
//                    if( ImGui.Selectable( $"{list[ manipIdx ].Eqp.Entry}##{manipIdx}" ) )
//                    {
//                        ImGui.OpenPopup( $"##MetaPopup{manipIdx}" );
//                    }
//
//                    break;
//                case MetaManipulation.Type.Gmp:
//                    changes = DrawGmpRow( manipIdx, list );
//                    ImGui.TableSetColumnIndex( 9 );
//                    if( ImGui.Selectable( $"{list[ manipIdx ].Gmp.Entry.Value}##{manipIdx}" ) )
//                    {
//                        ImGui.OpenPopup( $"##MetaPopup{manipIdx}" );
//                    }
//
//                    break;
//                case MetaManipulation.Type.Eqdp:
//                    changes = DrawEqdpRow( manipIdx, list );
//                    ImGui.TableSetColumnIndex( 9 );
//                    var (bit1, bit2) = GetEqdpBits( list[ manipIdx ].Eqdp.Slot, list[ manipIdx ].Eqdp.Entry );
//                    if( ImGui.Selectable( $"{bit1} {bit2}##{manipIdx}" ) )
//                    {
//                        ImGui.OpenPopup( $"##MetaPopup{manipIdx}" );
//                    }
//
//                    break;
//                case MetaManipulation.Type.Est:
//                    changes = DrawEstRow( manipIdx, list );
//                    ImGui.TableSetColumnIndex( 9 );
//                    if( ImGui.Selectable( $"{list[ manipIdx ].Est.Entry}##{manipIdx}" ) )
//                    {
//                        ImGui.OpenPopup( $"##MetaPopup{manipIdx}" );
//                    }
//
//                    break;
//                case MetaManipulation.Type.Imc:
//                    changes = DrawImcRow( manipIdx, list );
//                    ImGui.TableSetColumnIndex( 9 );
//                    if( ImGui.Selectable( $"{list[ manipIdx ].Imc.Entry.MaterialId}##{manipIdx}" ) )
//                    {
//                        ImGui.OpenPopup( $"##MetaPopup{manipIdx}" );
//                    }
//
//                    break;
//                case MetaManipulation.Type.Rsp:
//                    changes = DrawRspRow( manipIdx, list );
//                    ImGui.TableSetColumnIndex( 9 );
//                    if( ImGui.Selectable( $"{list[ manipIdx ].Rsp.Entry}##{manipIdx}" ) )
//                    {
//                        ImGui.OpenPopup( $"##MetaPopup{manipIdx}" );
//                    }
//
//                    break;
//            }
//
//
//            ImGui.TableNextRow();
//            return changes;
//        }
//
//
//        private MetaManipulation.Type DrawNewTypeSelection()
//        {
//            ImGui.RadioButton( "IMC##newManipType", ref _newManipTypeIdx, 1 );
//            ImGui.SameLine();
//            ImGui.RadioButton( "EQDP##newManipType", ref _newManipTypeIdx, 2 );
//            ImGui.SameLine();
//            ImGui.RadioButton( "EQP##newManipType", ref _newManipTypeIdx, 3 );
//            ImGui.SameLine();
//            ImGui.RadioButton( "EST##newManipType", ref _newManipTypeIdx, 4 );
//            ImGui.SameLine();
//            ImGui.RadioButton( "GMP##newManipType", ref _newManipTypeIdx, 5 );
//            ImGui.SameLine();
//            ImGui.RadioButton( "RSP##newManipType", ref _newManipTypeIdx, 6 );
//            return ( MetaManipulation.Type )_newManipTypeIdx;
//        }
//
//        private bool DrawNewManipulationPopup( string popupName, IList< MetaManipulation > list, ref int count )
//        {
//            var change = false;
//            if( !ImGui.BeginPopup( popupName ) )
//            {
//                return change;
//            }
//
//            using var         raii      = ImGuiRaii.DeferredEnd( ImGui.EndPopup );
//            var               manipType = DrawNewTypeSelection();
//            MetaManipulation? newManip  = null;
//            switch( manipType )
//            {
//                case MetaManipulation.Type.Imc:
//                {
//                    RestrictedInputInt( "Set Id##newManipImc", ref _newManipSetId, 0, ushort.MaxValue );
//                    RestrictedInputInt( "Variant##newManipImc", ref _newManipVariant, 0, byte.MaxValue );
//                    CustomCombo( "Object Type", ImcObjectType, out var objectType, ref _newManipObjectType );
//                    ImcManipulation imc = new();
//                    switch( objectType )
//                    {
//                        case ObjectType.Equipment:
//                            CustomCombo( "Equipment Slot", EqdpEquipSlots, out var equipSlot, ref _newManipEquipSlot );
//                            imc = new ImcManipulation( equipSlot, _newManipVariant, _newManipSetId, new ImcEntry() );
//                            break;
//                        case ObjectType.DemiHuman:
//                        case ObjectType.Weapon:
//                        case ObjectType.Monster:
//                            RestrictedInputInt( "Secondary Id##newManipImc", ref _newManipSecondaryId, 0, ushort.MaxValue );
//                            CustomCombo( "Body Slot", ImcBodySlots, out var bodySlot, ref _newManipBodySlot );
//                            imc = new ImcManipulation( objectType, bodySlot, _newManipSetId, _newManipSecondaryId,
//                                _newManipVariant, new ImcEntry() );
//                            break;
//                    }
//
//                    newManip = new MetaManipulation( new ImcManipulation( imc.ObjectType, imc.BodySlot, imc.PrimaryId, imc.SecondaryId,
//                        imc.Variant, imc.EquipSlot, ImcFile.GetDefault( imc.GamePath(), imc.EquipSlot, imc.Variant ) ) );
//
//                    break;
//                }
//                case MetaManipulation.Type.Eqdp:
//                {
//                    RestrictedInputInt( "Set Id##newManipEqdp", ref _newManipSetId, 0, ushort.MaxValue );
//                    CustomCombo( "Equipment Slot", EqdpEquipSlots, out var equipSlot, ref _newManipEquipSlot );
//                    CustomCombo( "Race", Races, out var race, ref _newManipRace );
//                    CustomCombo( "Gender", Genders, out var gender, ref _newManipGender );
//                    var eqdp = new EqdpManipulation( new EqdpEntry(), equipSlot, gender, race, _newManipSetId );
//                    newManip = new MetaManipulation( new EqdpManipulation( ExpandedEqdpFile.GetDefault( eqdp.FileIndex(), eqdp.SetId ),
//                        equipSlot, gender, race, _newManipSetId ) );
//                    break;
//                }
//                case MetaManipulation.Type.Eqp:
//                {
//                    RestrictedInputInt( "Set Id##newManipEqp", ref _newManipSetId, 0, ushort.MaxValue );
//                    CustomCombo( "Equipment Slot", EqpEquipSlots, out var equipSlot, ref _newManipEquipSlot );
//                    newManip = new MetaManipulation( new EqpManipulation( ExpandedEqpFile.GetDefault( _newManipSetId ) & Eqp.Mask( equipSlot ),
//                        equipSlot, _newManipSetId ) );
//                    break;
//                }
//                case MetaManipulation.Type.Est:
//                {
//                    RestrictedInputInt( "Set Id##newManipEst", ref _newManipSetId, 0, ushort.MaxValue );
//                    CustomCombo( "Est Type", EstTypes, out var estType, ref _newManipObjectType );
//                    CustomCombo( "Race", Races, out var race, ref _newManipRace );
//                    CustomCombo( "Gender", Genders, out var gender, ref _newManipGender );
//                    newManip = new MetaManipulation( new EstManipulation( gender, race, estType, _newManipSetId,
//                        EstFile.GetDefault( estType, Names.CombinedRace( gender, race ), _newManipSetId ) ) );
//                    break;
//                }
//                case MetaManipulation.Type.Gmp:
//                    RestrictedInputInt( "Set Id##newManipGmp", ref _newManipSetId, 0, ushort.MaxValue );
//                    newManip = new MetaManipulation( new GmpManipulation( ExpandedGmpFile.GetDefault( _newManipSetId ), _newManipSetId ) );
//                    break;
//                case MetaManipulation.Type.Rsp:
//                    CustomCombo( "Subrace", Subraces, out var subRace, ref _newManipSubrace );
//                    CustomCombo( "Attribute", RspAttributes, out var rspAttribute, ref _newManipAttribute );
//                    newManip = new MetaManipulation( new RspManipulation( subRace, rspAttribute,
//                        CmpFile.GetDefault( subRace, rspAttribute ) ) );
//                    break;
//            }
//
//            if( ImGui.Button( "Create Manipulation##newManip", Vector2.UnitX * -1 )
//            && newManip != null
//            && list.All( m => !m.Equals( newManip ) ) )
//            {
//                list.Add( newManip.Value );
//                change = true;
//                ++count;
//                ImGui.CloseCurrentPopup();
//            }
//
//            return change;
//        }
//
//        private bool DrawMetaManipulationsTable( string label, IList< MetaManipulation > list, ref int count )
//        {
//            var numRows = _editMode ? 11 : 10;
//            var changes = false;
//
//
//            if( list.Count > 0
//            && ImGui.BeginTable( label, numRows,
//                   ImGuiTableFlags.BordersInner | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit ) )
//            {
//                using var raii = ImGuiRaii.DeferredEnd( ImGui.EndTable );
//                if( _editMode )
//                {
//                    ImGui.TableNextColumn();
//                }
//
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Type##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Object Type##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Set##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Slot##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Race##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Gender##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Secondary ID##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Variant##{label}" );
//                ImGui.TableNextColumn();
//                ImGui.TableNextColumn();
//                ImGui.TableHeader( $"Value##{label}" );
//                ImGui.TableNextRow();
//
//                for( var i = 0; i < list.Count; ++i )
//                {
//                    changes |= DrawManipulationRow( ref i, list, ref count );
//                }
//            }
//
//            var popupName = $"##newManip{label}";
//            if( _editMode )
//            {
//                changes |= DrawNewManipulationPopup( $"##newManip{label}", list, ref count );
//                if( ImGui.Button( $"Add New Manipulation##{label}", Vector2.UnitX * -1 ) )
//                {
//                    ImGui.OpenPopup( popupName );
//                }
//
//                return changes;
//            }
//
//            return false;
//        }
//    }
//}