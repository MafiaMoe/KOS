﻿using kOS.Module;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using kOS.Suffixed.PartModuleField;
using kOS.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace kOS.Suffixed.Part
{
    public class PartValue : Structure, IKOSTargetable
    {
        protected SharedObjects Shared { get; private set; }

        public global::Part Part { get; private set; }

        public PartValue(global::Part part, SharedObjects sharedObj)
        {
            Part = part;
            Shared = sharedObj;

            // This cannot be called from inside InitializeSuffixes because the base constructor calls
            // InitializeSuffixes first before this constructor has set "Part" to a real value.
            PartInitializeSuffixes();
        }

        private void PartInitializeSuffixes()
        {
            AddSuffix("CONTROLFROM", new NoArgsSuffix(ControlFrom));
            AddSuffix("NAME", new Suffix<string>(() => Part.name));
            AddSuffix("FUELCROSSFEED", new Suffix<bool>(() => Part.fuelCrossFeed));
            AddSuffix("TITLE", new Suffix<string>(() => Part.partInfo.title));
            AddSuffix("STAGE", new Suffix<int>(() => Part.inverseStage));
            AddSuffix("UID", new Suffix<string>(Part.flightID.ToString));
            AddSuffix("ROTATION", new Suffix<Direction>(() => new Direction(Part.transform.rotation)));
            AddSuffix("POSITION", new Suffix<Vector>(() => new Vector(Part.transform.position - Shared.Vessel.findWorldCenterOfMass())));
            AddSuffix("TAG", new SetSuffix<string>(GetTagName, SetTagName));
            AddSuffix("FACING", new Suffix<Direction>(() => GetFacing(Part)));
            AddSuffix("RESOURCES", new Suffix<ListValue>(() => GatherResources(Part)));
            AddSuffix("TARGETABLE", new Suffix<bool>(() => Part.Modules.OfType<ITargetable>().Any()));
            AddSuffix("SHIP", new Suffix<VesselTarget>(() => new VesselTarget(Part.vessel, Shared)));
            AddSuffix("HASMODULE", new OneArgsSuffix<bool, string>(HasModule));
            AddSuffix("GETMODULE", new OneArgsSuffix<PartModuleFields, string>(GetModule));
            AddSuffix("GETMODULEBYINDEX", new OneArgsSuffix<PartModuleFields, int>(GetModuleIndex));
            AddSuffix(new[] { "MODULES", "ALLMODULES" }, new Suffix<ListValue>(GetAllModules, "A List of all the modules' names on this part"));
            AddSuffix("PARENT", new Suffix<PartValue>(() => PartValueFactory.Construct(Part.parent, Shared), "The parent part of this part"));
            AddSuffix("HASPARENT", new Suffix<bool>(() => Part.parent != null, "Tells you if this part has a parent, is used to avoid null exception from PARENT"));
            AddSuffix("CHILDREN", new Suffix<ListValue<PartValue>>(() => PartValueFactory.ConstructGeneric(Part.children, Shared), "A LIST() of the children parts of this part"));
            AddSuffix("DRYMASS", new Suffix<float>(Part.GetDryMass, "The Part's mass when empty"));
            AddSuffix("MASS", new Suffix<float>(Part.CalculateCurrentMass, "The Part's current mass"));
            AddSuffix("WETMASS", new Suffix<float>(Part.GetWetMass, "The Part's mass when full"));
            AddSuffix("HASPHYSICS", new Suffix<bool>(Part.HasPhysics, "Is this a strange 'massless' part"));
        }

        public void ThrowIfNotCPUVessel()
        {
            if (Part.vessel.id != Shared.Vessel.id)
                throw new KOSWrongCPUVesselException();
        }

        private PartModuleFields GetModule(string modName)
        {
            foreach (PartModule mod in Part.Modules)
            {
                if (string.Equals(mod.moduleName, modName, StringComparison.OrdinalIgnoreCase))
                {
                    return PartModuleFieldsFactory.Construct(mod, Shared);
                }
            }
            throw new KOSLookupFailException("module", modName.ToUpper(), this);
        }

        private bool HasModule(string modName)
        {
            foreach (PartModule mod in Part.Modules)
            {
                if (string.Equals(mod.moduleName, modName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private PartModuleFields GetModuleIndex(int moduleIndex)
        {
            if (moduleIndex < Part.Modules.Count)
            {
                return PartModuleFieldsFactory.Construct(Part.Modules.GetModule(moduleIndex), Shared);
            }
            throw new KOSLookupFailException("module", string.Format("MODULEINDEX[{0}]", moduleIndex), this);
        }

        public string GetTagName() // public because I picture this being a useful API method later
        {
            KOSNameTag tagModule = Part.Modules.OfType<KOSNameTag>().FirstOrDefault();
            return tagModule == null ? string.Empty : tagModule.nameTag;
        }

        private void SetTagName(string value)
        {
            ThrowIfNotCPUVessel();
            KOSNameTag tagModule = Part.Modules.OfType<KOSNameTag>().FirstOrDefault();
            if (tagModule != null) tagModule.nameTag = value;
        }

        public override string ToString()
        {
            string tagName = GetTagName();
            if (string.IsNullOrEmpty(tagName))
                return string.Format("PART({0},uid={1})", Part.name, Part.uid());
            return string.Format("PART({0},tag={1})", Part.name, tagName);
        }

        public virtual ITargetable Target
        {
            get
            {
                return Part.Modules.OfType<ITargetable>().FirstOrDefault();
            }
        }

        private Direction GetFacing(global::Part part)
        {
            // Our normal facings use Z for forward, but parts use Y for forward:
            Quaternion rotateZToY = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            Quaternion newRotation = part.transform.rotation * rotateZToY;
            return new Direction(newRotation);
        }

        private void ControlFrom()
        {
            ThrowIfNotCPUVessel();
            var dockingModule = Part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
            var commandModule = Part.Modules.OfType<ModuleCommand>().FirstOrDefault();

            if (commandModule != null)
            {
                commandModule.MakeReference();
            }
            else if (dockingModule != null)
            {
                dockingModule.MakeReferenceTransform();
            }
            else
            {
                throw new KOSCommandInvalidHereException("CONTROLFROM", "a generic part value", "a docking port or command part");
            }
        }

        private ListValue GatherResources(global::Part part)
        {
            var resources = new ListValue();
            foreach (PartResource resource in part.Resources)
            {
                resources.Add(new SingleResourceValue(resource));
            }
            return resources;
        }

        private ListValue GetAllModules()
        {
            var returnValue = new ListValue();
            foreach (PartModule mod in Part.Modules)
            {
                returnValue.Add(mod.moduleName);
            }
            return returnValue;
        }

        protected bool Equals(PartValue other)
        {
            return Equals(Part, other.Part);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PartValue)obj);
        }

        public override int GetHashCode()
        {
            return (Part != null ? Part.GetHashCode() : 0);
        }

        public static bool operator ==(PartValue left, PartValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PartValue left, PartValue right)
        {
            return !Equals(left, right);
        }
    }
}
