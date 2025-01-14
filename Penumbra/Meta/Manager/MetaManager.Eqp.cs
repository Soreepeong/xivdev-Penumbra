using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Penumbra.Interop.Structs;
using Penumbra.Meta.Files;
using Penumbra.Meta.Manipulations;
using Penumbra.Mods;

namespace Penumbra.Meta.Manager;

public partial class MetaManager
{
    public struct MetaManagerEqp : IDisposable
    {
        public          ExpandedEqpFile?                   File          = null;
        public readonly Dictionary< EqpManipulation, IMod > Manipulations = new();

        public MetaManagerEqp()
        { }

        [Conditional( "USE_EQP" )]
        public void SetFiles()
            => SetFile( File, CharacterUtility.EqpIdx );

        [Conditional( "USE_EQP" )]
        public static void ResetFiles()
            => SetFile( null, CharacterUtility.EqpIdx );

        [Conditional( "USE_EQP" )]
        public void Reset()
        {
            if( File == null )
            {
                return;
            }

            File.Reset( Manipulations.Keys.Select( m => ( int )m.SetId ) );
            Manipulations.Clear();
        }

        public bool ApplyMod( EqpManipulation m, IMod mod )
        {
#if USE_EQP
            Manipulations[ m ] =   mod;
            File               ??= new ExpandedEqpFile();
            return m.Apply( File );
#else
            return false;
#endif
        }

        public bool RevertMod( EqpManipulation m )
        {
#if USE_EQP
            if( Manipulations.Remove( m ) )
            {
                var def   = ExpandedEqpFile.GetDefault( m.SetId );
                var manip = new EqpManipulation( def, m.Slot, m.SetId );
                return manip.Apply( File! );
            }
#endif
            return false;
        }

        public void Dispose()
        {
            File?.Dispose();
            File = null;
            Manipulations.Clear();
        }
    }
}