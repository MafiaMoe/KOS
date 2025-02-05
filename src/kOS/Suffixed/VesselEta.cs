﻿using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;

namespace kOS.Suffixed
{
    public class VesselEta : Structure
    {
        private readonly SharedObjects shared;
        public VesselEta(SharedObjects shared )
        {
            this.shared = shared;
            InitializeSuffixEta();
        }

        private void InitializeSuffixEta()
        {
            AddSuffix("APOAPSIS" , new NoArgsSuffix<double>(GetApoapsis));
            AddSuffix("PERIAPSIS" , new NoArgsSuffix<double>(GetPeriapsis));
            AddSuffix("TRANSITION" , new NoArgsSuffix<double>(GetTransition));
        }
        public double GetApoapsis()
        {
            return shared.Vessel.orbit.timeToAp;            
        }
        
        public double GetPeriapsis()
        {
            return shared.Vessel.orbit.timeToPe;
        }
        
        public double GetTransition()
        {
            return shared.Vessel.orbit.EndUT - Planetarium.GetUniversalTime();
        }
        
        public override string ToString()
        {
            return string.Format("ETA: Apoapsis={0} Periapsis={1} Transition={2}", GetApoapsis(), GetPeriapsis(), GetTransition());
        }
    }
}