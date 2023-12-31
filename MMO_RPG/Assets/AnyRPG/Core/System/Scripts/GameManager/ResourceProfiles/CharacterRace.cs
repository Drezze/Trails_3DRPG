using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Character Race", menuName = "AnyRPG/CharacterRace")]
    public class CharacterRace : DescribableResource, IStatProvider, ICapabilityProvider, IEnvironmentPreviewSource {

        [Header("NewGame")]

        [Tooltip("If true, this race is available for Players to choose on the new game menu, no matter what faction is chosen.")]
        [SerializeField]
        private bool newGameOption = false;

        [Tooltip("The unit profile to use when the male gender is chosen.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string maleUnitProfile = string.Empty;

        private UnitProfile maleUnitProfileRef = null;

        [Tooltip("The unit profile to use when the female gender is chosen.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(UnitProfile))]
        private string femaleUnitProfile = string.Empty;

        private UnitProfile femaleUnitProfileRef = null;

        [SerializeField]
        private EnvironmentPreviewProperties environmentPreview = new EnvironmentPreviewProperties();

        [Header("Start Equipment")]

        [Tooltip("The names of the equipment that will be worn by this race when a new game is started")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Equipment))]
        private List<string> equipmentNames = new List<string>();

        private List<Equipment> equipmentList = new List<Equipment>();

        [Header("Attack Effect Defaults")]

        [Tooltip("Ability effects to cast on the target when the character does not have a weapon equipped and does damage from a standard (auto) attack")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> defaultHitEffects = new List<string>();

        private List<AbilityEffect> defaultHitEffectList = new List<AbilityEffect>();

        [Tooltip("Ability effects to cast on the target when the weapon does damage from any attack, including standard (auto) attacks")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        private List<string> onHitEffects = new List<string>();

        private List<AbilityEffect> onHitEffectList = new List<AbilityEffect>();

        [Header("Capabilities")]

        [Tooltip("Capabilities that apply to all characters of this class")]
        [SerializeField]
        private CapabilityProps capabilities = new CapabilityProps();

        [Header("Stats and Scaling")]

        [Tooltip("Stats available to this character class, in addition to the stats defined at the system level that all character use")]
        [FormerlySerializedAs("statScaling")]
        [SerializeField]
        private List<StatScalingNode> primaryStats = new List<StatScalingNode>();

        [Header("Power Resources")]

        [Tooltip("Power Resources used by this class.  The first resource is considered primary and will show on the unit frame.")]
        [SerializeField]
        private List<string> powerResources = new List<string>();

        // reference to the actual power resources
        private List<PowerResource> powerResourceList = new List<PowerResource>();

        public List<PowerResource> PowerResourceList { get => powerResourceList; set => powerResourceList = value; }
        public List<StatScalingNode> PrimaryStats { get => primaryStats; set => primaryStats = value; }
        public List<Equipment> EquipmentList { get => equipmentList; set => equipmentList = value; }
        public List<AbilityEffect> DefaultHitEffectList { get => defaultHitEffectList; set => defaultHitEffectList = value; }
        public List<AbilityEffect> OnHitEffectList { get => onHitEffectList; set => onHitEffectList = value; }
        public CapabilityProps Capabilities { get => capabilities; set => capabilities = value; }
        public bool NewGameOption { get => newGameOption; set => newGameOption = value; }
        public UnitProfile MaleUnitProfile { get => maleUnitProfileRef; }
        public UnitProfile FemaleUnitProfile { get => femaleUnitProfileRef; }
        public EnvironmentPreviewProperties EnvironmentPreview { get => environmentPreview; set => environmentPreview = value; }

        public CapabilityProps GetFilteredCapabilities(ICapabilityConsumer capabilityConsumer, bool returnAll = true) {
            return capabilities;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (maleUnitProfile != string.Empty) {
                maleUnitProfileRef = systemDataFactory.GetResource<UnitProfile>(maleUnitProfile);
                if (maleUnitProfileRef == null) {
                    Debug.LogError("CharacterRace.SetupScriptableObjects(): Could not find unit profile : " + maleUnitProfile + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

            if (femaleUnitProfile != string.Empty) {
                femaleUnitProfileRef = systemDataFactory.GetResource<UnitProfile>(femaleUnitProfile);
                if (femaleUnitProfileRef == null) {
                    Debug.LogError("CharacterRace.SetupScriptableObjects(): Could not find unit profile : " + femaleUnitProfile + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

            if (onHitEffects != null) {
                foreach (string onHitEffectName in onHitEffects) {
                    if (onHitEffectName != null && onHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(onHitEffectName);
                        if (abilityEffect != null) {
                            onHitEffectList.Add(abilityEffect);
                        } else {
                            Debug.LogError("CharacterRace.SetupScriptableObjects(): Could not find ability effect : " + onHitEffectName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("CharacterRace.SetupScriptableObjects(): null or empty on hit effect found while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (defaultHitEffects != null) {
                foreach (string defaultHitEffectName in defaultHitEffects) {
                    if (defaultHitEffectName != null && defaultHitEffectName != string.Empty) {
                        AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(defaultHitEffectName);
                        if (abilityEffect != null) {
                            defaultHitEffectList.Add(abilityEffect);
                        } else {
                            Debug.LogError("CharacterRace.SetupScriptableObjects(): Could not find ability effect : " + defaultHitEffectName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                        }
                    } else {
                        Debug.LogError("CharacterRace.SetupScriptableObjects(): null or empty default hit effect found while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            if (equipmentNames != null) {
                foreach (string equipmentName in equipmentNames) {
                    Equipment tmpEquipment = null;
                    tmpEquipment = systemDataFactory.GetResource<Item>(equipmentName) as Equipment;
                    if (tmpEquipment != null) {
                        equipmentList.Add(tmpEquipment);
                    } else {
                        Debug.LogError("CharacterRace.SetupScriptableObjects(): Could not find equipment : " + equipmentName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            powerResourceList = new List<PowerResource>();
            if (powerResources != null) {
                foreach (string powerResourcename in powerResources) {
                    PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourcename);
                    if (tmpPowerResource != null) {
                        powerResourceList.Add(tmpPowerResource);
                    } else {
                        Debug.LogError("CharacterRace.SetupScriptableObjects(): Could not find power resource : " + powerResourcename + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in primaryStats) {
                statScalingNode.SetupScriptableObjects(systemDataFactory);
            }

            capabilities.SetupScriptableObjects(systemDataFactory);

        }

    }

}