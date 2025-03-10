//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	using System;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Domain.Results;


	/// <summary>
	/// Class for holding item information
	/// </summary>
	public class Item
	{

		#region Item Fields

		/// <summary>
		/// Default value for empty var2.
		/// </summary>
		public const int var2Default = 2035248;

		/// <summary>
		/// Random number used as a seed for new items
		/// </summary>
		private static Random random = new Random();

		/// <summary>
		/// Binary marker for the beginning of a block
		/// </summary>
		public int beginBlockCrap1;

		/// <summary>
		/// Binary marker for the end of a block
		/// </summary>
		public int endBlockCrap1;

		/// <summary>
		/// A different binary marker for the beginning of a block
		/// </summary>
		public int beginBlockCrap2;

		/// <summary>
		/// A different binary marker for the end of a block
		/// </summary>
		public int endBlockCrap2;

		public bool atlantis = false;

		/// <summary>
		/// Prefix database record ID
		/// </summary>
		public string prefixID;

		public bool HasPrefix
			=> !string.IsNullOrWhiteSpace(prefixID);

		/// <summary>
		/// Suffix database record ID
		/// </summary>
		public string suffixID;

		public bool HasSuffix
			=> !string.IsNullOrWhiteSpace(suffixID);

		/// <summary>
		/// Gets the value indicating whether the item allows 2 relic socketing
		/// </summary>
		public bool AcceptExtraRelic
			=> HasSuffix && suffixID.ToUpper().EndsWith("RARE_EXTRARELIC_01.DBR");

		/// <summary>
		/// Relic database record ID
		/// </summary>
		public string relicID;

		/// <summary>
		/// Relic database record ID
		/// </summary>
		public string relic2ID;

		/// <summary>
		/// Info structure for the base item
		/// </summary>
		public Info baseItemInfo;

		/// <summary>
		/// Info structure for the item's prefix
		/// </summary>
		public Info prefixInfo;

		/// <summary>
		/// Info structure for the item's suffix
		/// </summary>
		public Info suffixInfo;

		/// <summary>
		/// Used for level calculation
		/// </summary>
		public int attributeCount;

		/// <summary>
		/// Used so that attributes are not counted multiple times
		/// </summary>
		public bool isAttributeCounted;

		/// <summary>
		/// Used for itemScalePercent calculation
		/// </summary>
		public float itemScalePercent;

		#endregion Item Fields

		/// <summary>
		/// Initializes a new instance of the Item class.
		/// </summary>
		public Item()
		{
			// Added by VillageIdiot
			// Used for bare item attributes in properties display in this order:
			// baseinfo, artifactCompletionBonus, prefixinfo, suffixinfo, relicinfo, relicCompletionBonus
			this.itemScalePercent = 1.00F;
			this.StackSize = 1;
		}


		public string GameExtensionSuffix
		{
			get
			{
				string ext = ItemStyle.Quest.TQColor().ColorTag();
				if (this.IsImmortalThrone) ext += "(IT)";
				else if (this.IsRagnarok) ext += "(RAG)";
				else if (this.IsAtlantis) ext += "(ATL)";
				else if (this.IsEmbers) ext += "(EEM)";
				return ext;
			}
		}

		#region Item Properties

		/// <summary>
		/// Gets the weapon slot indicator value.
		/// This is a special value in the coordinates that signals an item is in a weapon slot.
		/// </summary>
		public const int WeaponSlotIndicator = -3;

		/// <summary>
		/// Gets the base item id
		/// </summary>
		public string BaseItemId { get; set; }

		/// <summary>
		/// Gets or sets the relic bonus id
		/// </summary>
		public string RelicBonusId { get; set; }

		/// <summary>
		/// Gets or sets the relic bonus2 id
		/// </summary>
		public string RelicBonus2Id { get; set; }

		/// <summary>
		/// Gets or sets the item seed
		/// </summary>
		public int Seed { get; set; }

		/// <summary>
		/// Gets the number of relics
		/// </summary>
		private int var1;

		/// <summary>
		/// Last <see cref="ToFriendlyNameResult"/> queried for this item.
		/// </summary>
		public ToFriendlyNameResult CurrentFriendlyNameResult;

		public int Var1
		{
			get
			{
				// The "Power of Nerthus" relic is a special quest-reward relic with only 1 shard (it is always complete). 
				// Since its database var1 value is 0, we hard-set it to 1.
				if (IsRelic && var1 == 0)
					return 1;

				return var1;
			}

			set => var1 = value;
		}

		/// <summary>
		/// Gets the number of relics for the second relic slot
		/// </summary>
		public int Var2 { get; set; }

		/// <summary>
		/// Gets or sets the stack size.
		/// Used for stackable items like potions.
		/// </summary>
		public int StackSize { get; set; }

		/// <summary>
		/// Gets or sets the X cell position of the item.
		/// </summary>
		public int PositionX { get; set; }

		/// <summary>
		/// Gets or sets the Y cell position of the item.
		/// </summary>
		public int PositionY { get; set; }

		/// <summary>
		/// Gets or sets the item's upper left corner in cell coordinates.
		/// </summary>
		public Point Location
		{
			get => new Point(this.PositionX, this.PositionY);

			set
			{
				this.PositionX = value.X;
				this.PositionY = value.Y;
			}
		}

		/// <summary>
		/// Gets the relic info
		/// </summary>
		public Info RelicInfo { get; set; }

		/// <summary>
		/// Gets or sets the relic bonus info
		/// </summary>
		public Info RelicBonusInfo { get; set; }

		/// <summary>
		/// Gets the second relic info
		/// </summary>
		public Info Relic2Info { get; set; }

		/// <summary>
		/// Gets or sets the second relic bonus info
		/// </summary>
		public Info RelicBonus2Info { get; set; }

		/// <summary>
		/// Raw image data from game resource file
		/// </summary>
		public byte[] TexImage { get; set; }

		/// <summary>
		/// ResourceId related to <see cref="TexImage"/>
		/// </summary>
		public string TexImageResourceId { get; set; }

		/// <summary>
		/// Gets the item's width in cells
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Gets the item's height in cells.
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Gets the item's size in cells.
		/// </summary>
		public Size Size => new Size(this.Width, this.Height);

		/// <summary>
		/// Gets the item's right cell location
		/// </summary>
		public int Right => this.Location.X + this.Width;

		/// <summary>
		/// Gets the item's bottom cell location.
		/// </summary>
		public int Bottom => this.Location.Y + this.Height;


		/// <summary>
		/// Gets or sets the item container type
		/// </summary>
		public SackType ContainerType { get; set; }

		/// <summary>
		/// Gets a value indicating whether the item is in an equipment weapon slot.
		/// </summary>
		public bool IsInWeaponSlot => this.PositionX == WeaponSlotIndicator;

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Immortal Throne expansion pack.
		/// </summary>
		public bool IsImmortalThrone
			=> this.BaseItemId.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0;

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Ragnarok DLC.
		/// </summary>
		public bool IsRagnarok
			=> this.BaseItemId.ToUpperInvariant().IndexOf("XPACK2\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK2\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK2\\", StringComparison.OrdinalIgnoreCase) >= 0;

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Atlantis DLC.
		/// </summary>
		public bool IsAtlantis
			=> this.BaseItemId.ToUpperInvariant().IndexOf("XPACK3\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK3\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK3\\", StringComparison.OrdinalIgnoreCase) >= 0;

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Eternal Embers DLC.
		/// </summary>
		public bool IsEmbers
			=> this.BaseItemId.ToUpperInvariant().IndexOf("XPACK4\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK4\\", StringComparison.OrdinalIgnoreCase) >= 0
			|| this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK4\\", StringComparison.OrdinalIgnoreCase) >= 0;


		/// <summary>
		/// Gets a value indicating whether the item is a scroll.
		/// </summary>
		public bool IsScroll
		{
			get
			{
				if (this.baseItemInfo != null)
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_SCROLL");
				else if ((this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis || this.IsEmbers)
					&& (this.BaseItemId.ToUpperInvariant().IndexOf("\\SCROLLS\\", StringComparison.OrdinalIgnoreCase) >= 0))
					return true;

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a parchment.
		/// </summary>
		public bool IsParchment
		{
			get
			{
				if ((this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis || this.IsEmbers)
					&& (this.BaseItemId.ToUpperInvariant().IndexOf("PARCHMENT", StringComparison.OrdinalIgnoreCase) >= 0))
					return true;

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a formulae.
		/// </summary>
		public bool IsFormulae
		{
			get
			{
				if (this.baseItemInfo != null)
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMARTIFACTFORMULA");
				else if ((this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis || this.IsEmbers)
					&& (this.BaseItemId.ToUpperInvariant().IndexOf("\\ARCANEFORMULAE\\", StringComparison.OrdinalIgnoreCase) >= 0))
					return true;

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is an artifact.
		/// </summary>
		public bool IsArtifact
		{
			get
			{
				if (this.baseItemInfo != null)
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMARTIFACT");
				else if ((this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis || this.IsEmbers)
					&& (!this.IsFormulae && this.BaseItemId.ToUpperInvariant().IndexOf("\\ARTIFACTS\\", StringComparison.OrdinalIgnoreCase) >= 0))
					return true;

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a shield.
		/// </summary>
		public bool IsShield
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONARMOR_SHIELD") : false;

		/// <summary>
		/// Gets a value indicating whether the item is armor.
		/// </summary>
		public bool IsArmor
			=> (this.baseItemInfo != null)
			? this.baseItemInfo.ItemClass.ToUpperInvariant().StartsWith("ARMORPROTECTIVE", StringComparison.OrdinalIgnoreCase)
			: false;

		/// <summary>
		/// Gets a value indicating whether the item is a helm.
		/// </summary>
		public bool IsHelm
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_HEAD") : false;

		/// <summary>
		/// Gets a value indicating whether the item is a bracer.
		/// </summary>
		public bool IsBracer
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_FOREARM") : false;

		/// <summary>
		/// Gets a value indicating whether the item is torso armor.
		/// </summary>
		public bool IsTorsoArmor
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_UPPERBODY") : false;

		/// <summary>
		/// Gets a value indicating whether the item is a greave.
		/// </summary>
		public bool IsGreave
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_LOWERBODY") : false;

		/// <summary>
		/// Gets a value indicating whether the item is Jewellery.
		/// </summary>
		public bool IsJewellery
			=> IsRing || IsAmulet;

		/// <summary>
		/// Gets a value indicating whether the item is a ring.
		/// </summary>
		public bool IsRing
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORJEWELRY_RING") : false;

		/// <summary>
		/// Gets a value indicating whether the item is an amulet.
		/// </summary>
		public bool IsAmulet
			=> (this.baseItemInfo != null) ? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORJEWELRY_AMULET") : false;

		/// <summary>
		/// Gets a value indicating whether the item is a weapon.
		/// </summary>
		public bool IsWeapon
			=> (this.baseItemInfo != null)
			? !this.IsShield && this.baseItemInfo.ItemClass.ToUpperInvariant().StartsWith("WEAPON", StringComparison.OrdinalIgnoreCase)
			: false;

		/// <summary>
		/// Gets a value indicating whether the item is a thrown weapon.
		/// </summary>
		public bool IsThrownWeapon
			=> (this.baseItemInfo != null)
			? this.baseItemInfo.ItemClass.Equals("WeaponHunting_RangedOneHand", StringComparison.OrdinalIgnoreCase)
			: false;

		/// <summary>
		/// Gets a value indicating whether the item is a two handed weapon.
		/// </summary>
		public bool Is2HWeapon
			=> (this.baseItemInfo != null)
			? this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONHUNTING_BOW") || this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONMAGICAL_STAFF")
			: false;

		/// <summary>
		/// Gets a value indicating whether the item is a weapon or shield.
		/// </summary>
		public bool IsWeaponShield
			=> (this.baseItemInfo != null)
			? this.baseItemInfo.ItemClass.ToUpperInvariant().StartsWith("WEAPON", StringComparison.OrdinalIgnoreCase)
			: false;

		const string QUEST = "QUEST";
		const string QUESTS = "QUESTS";
		const string QUESTITEM = "QUESTITEM";
		/// <summary>
		/// Gets a value indicating whether the item is a quest item.
		/// </summary>
		public bool IsQuestItem
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals(QUEST)
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals(QUESTITEM))
						return true;
				}
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis && !this.IsEmbers)
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf(QUEST, StringComparison.OrdinalIgnoreCase) >= 0)
						return true;
				}
				else if (this.BaseItemId.ToUpperInvariant().IndexOf(QUESTS, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Get a value indicating gear level from <see cref="GearLevel.Broken"/> to <see cref="GearLevel.Legendary"/>
		/// </summary>
		public GearLevel GearLevel
		{
			get
			{
				switch (ItemStyle)
				{
					case ItemStyle.Broken:
						return GearLevel.Broken;
					case ItemStyle.Mundane:
						return GearLevel.Mundane;
					case ItemStyle.Common:
						return GearLevel.Common;
					case ItemStyle.Rare:
						return GearLevel.Rare;
					case ItemStyle.Epic:
						return GearLevel.Epic;
					case ItemStyle.Legendary:
						return GearLevel.Legendary;
					case ItemStyle.Quest:
					case ItemStyle.Relic:
					case ItemStyle.Potion:
					case ItemStyle.Scroll:
					case ItemStyle.Parchment:
					case ItemStyle.Formulae:
					case ItemStyle.Artifact:
					default:
						return GearLevel.NoGear;
				}
			}
		}

		/// <summary>
		/// Gets the item style enumeration
		/// </summary>
		public ItemStyle ItemStyle
		{
			get
			{
				if (this.prefixInfo != null && this.prefixInfo.ItemClassification.ToUpperInvariant().Equals("BROKEN"))
					return ItemStyle.Broken;

				if (this.IsArtifact)
					return ItemStyle.Artifact;

				if (this.IsFormulae)
					return ItemStyle.Formulae;

				if (this.IsScroll)
					return ItemStyle.Scroll;

				if (this.IsParchment)
					return ItemStyle.Parchment;

				if (this.IsRelic)
					return ItemStyle.Relic;

				if (this.IsPotion)
					return ItemStyle.Potion;

				if (this.IsQuestItem)
					return ItemStyle.Quest;

				if (this.baseItemInfo != null)
				{
					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("EPIC"))
						return ItemStyle.Epic;

					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("LEGENDARY"))
						return ItemStyle.Legendary;

					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("RARE"))
						return ItemStyle.Rare;
				}

				// At this point baseItem indicates Common.  Let's check affixes
				if (this.suffixInfo != null && this.suffixInfo.ItemClassification.ToUpperInvariant().Equals("RARE"))
					return ItemStyle.Rare;

				if (this.prefixInfo != null && this.prefixInfo.ItemClassification.ToUpperInvariant().Equals("RARE"))
					return ItemStyle.Rare;

				// Not rare.  If we have a suffix or prefix, then call it common, else mundane
				if (this.suffixInfo != null || this.prefixInfo != null)
					return ItemStyle.Common;

				return ItemStyle.Mundane;
			}
		}

		/// <summary>
		/// Gets the item class
		/// </summary>
		public string ItemClass
			=> (this.baseItemInfo == null) ? string.Empty : this.baseItemInfo.ItemClass;

		/// <summary>
		/// Gets a value indicating whether or not the item will stack.
		/// </summary>
		public bool DoesStack => this.IsPotion || this.IsScroll;

		/// <summary>
		/// Gets a value indicating whether the item has a number attached.
		/// </summary>
		public bool HasNumber => this.DoesStack || (this.IsRelic && !this.IsRelicComplete);

		/// <summary>
		/// Gets or sets the number attached to the item.
		/// </summary>
		public int Number
		{
			get
			{
				if (this.DoesStack)
					return this.StackSize;

				if (this.IsRelic)
					return Math.Max(this.Var1, 1);

				return 0;
			}

			set
			{
				// Added by VillageIdiot
				if (this.DoesStack)
					this.StackSize = value;

				else if (this.IsRelic)
				{
					// Limit value to complete Relic level
					if (value >= this.baseItemInfo.CompletedRelicLevel)
						this.Var1 = this.baseItemInfo.CompletedRelicLevel;
					else
						this.Var1 = value;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item has an embedded relic.
		/// </summary>
		public bool HasRelicSlot1 => !this.IsRelic && this.relicID.Length > 0;

		/// <summary>
		/// Gets a value indicating whether the item has a second embedded relic.
		/// </summary>
		public bool HasRelicSlot2 => !this.IsRelic && this.relic2ID.Length > 0;

		/// <summary>
		/// Indicate that the item has an embedded relic.
		/// </summary>
		public bool HasRelic => HasRelicSlot1 | HasRelicSlot2;

		/// <summary>
		/// Gets a value indicating whether the item is a potion.
		/// </summary>
		public bool IsPotion
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_POTIONHEALTH")
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_POTIONMANA")
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_SCROLL_ETERNAL"); //AMS: New EE Potions (Mystical Potions)
				}

				return this.BaseItemId.ToUpperInvariant().IndexOf("ONESHOT\\POTION", StringComparison.OrdinalIgnoreCase) != -1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a charm and only a charm.
		/// </summary>
		public bool IsCharm
		{
			get
			{
				if (this.baseItemInfo != null)
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis && !this.IsEmbers)
					return this.BaseItemId.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) != -1;
				else
					return (this.BaseItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) != -1);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item first relic is a charm and only a charm.
		/// </summary>
		public bool IsRelic1Charm
		{
			get
			{
				if (this.RelicInfo != null)
					return this.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis && !this.IsEmbers)
					return (this.RelicInfo?.ItemId.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
				else
					return (this.RelicInfo?.ItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item second relic is a charm and only a charm.
		/// </summary>
		public bool IsRelic2Charm
		{
			get
			{
				if (this.Relic2Info != null)
					return this.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis && !this.IsEmbers)
					return (this.Relic2Info?.ItemId.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
				else
					return (this.Relic2Info?.ItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a relic or a charm.
		/// </summary>
		public bool IsRelic
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMRELIC")
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				}
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis && !this.IsEmbers)
					return this.BaseItemId.ToUpperInvariant().IndexOf("RELICS", StringComparison.OrdinalIgnoreCase) != -1;
				else
				{
					return (
						(this.BaseItemId.ToUpperInvariant().IndexOf("\\RELICS\\", StringComparison.OrdinalIgnoreCase) != -1)
						|| (this.BaseItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) != -1)
					);
				}
			}
		}

		/// <summary>
		/// Indicate if this item is a completed relic.
		/// </summary>
		public bool IsRelicComplete
			=> (this.baseItemInfo != null) ? this.Var1 >= this.baseItemInfo.CompletedRelicLevel : false;

		/// <summary>
		/// Indicate if the first relic completion bonus apply.
		/// </summary>
		public bool IsRelicBonus1Complete
			=> (this.RelicBonusInfo != null) ? this.Var1 >= this.RelicBonusInfo.CompletedRelicLevel : false;

		/// <summary>
		/// Indicate if the second relic completion bonus apply.
		/// </summary>
		public bool IsRelicBonus2Complete
			=> (this.RelicBonus2Info != null) ? this.Var2 >= this.RelicBonus2Info.CompletedRelicLevel : false;

		internal static ReadOnlyCollection<(string ItemClass, string xTagName)> ItemClassMap = new[]
		{
			("QUESTITEM", "tagQuestItem"),
			("ONESHOT_POTIONHEALTH", "tagHUDHealthPotion"),
			("ONESHOT_POTIONMANA", "tagHUDEnergyPotion"),
			("ONESHOT_SCROLL_ETERNAL", "x4tag_PotionReward"), // Translate into: Mystical Potion
			("ARMORJEWELRY_AMULET", "tagItemAmulet") ,
			("ARMORJEWELRY_RING", "tagItemRing") ,
			("ITEMCHARM", "tagItemCharm") ,
			("ITEMRELIC", "tagRelic"),
			("ITEMARTIFACTFORMULA", "xtagEnchant02"),
			("ITEMARTIFACT", "tagArtifact"),
			("ONESHOT_SCROLL", "xtagLogScroll"),
			("ARMORPROTECTIVE_LOWERBODY", "tagCR_Leg"),
			("ARMORPROTECTIVE_FOREARM", "tagCR_Arm"),
			("ARMORPROTECTIVE_HEAD", "tagCR_Head"),
			("ARMORPROTECTIVE_UPPERBODY", "tagCR_Torso"),
			("WEAPONARMOR_SHIELD", "tagItemWarShield"),
			("WEAPONMELEE_AXE", "tagItemAxe"),
			("WEAPONMELEE_MACE", "tagItemMace"),
			("WEAPONMELEE_SWORD", "tagItemWarBlade"),//tagSword
			("WEAPONHUNTING_BOW", "tagItemWarBow"),
			("WEAPONHUNTING_RANGEDONEHAND", "x2tagThrownWeapon"),
			("WEAPONMAGICAL_STAFF", "tagItemBattleStaff"),// xtagLogStaff
			("WEAPONHUNTING_SPEAR","tagItemLance"),
			("ITEMEQUIPMENT","xtagArtifactReagentTypeEquipment"),
			("ONESHOT_DYE","tagDye"),
		}.ToList().AsReadOnly();

		/// <summary>
		/// Get the tagName assigned to the <see cref="ItemClass"/>
		/// </summary>
		public string ItemClassTagName
			=> GetClassTagName(this.baseItemInfo.ItemClass);

		/// <summary>
		/// Get the tagName assigned to the <see cref="ItemClass"/>
		/// </summary>
		public static string GetClassTagName(string ItemClass)
		{
			var iclass = ItemClass.ToUpperInvariant();
			var map = ItemClassMap.Where(i => i.ItemClass == iclass).Select(i => i.xTagName);
			return map.Any() ? map.First() : ItemClass;
		}

		/// <summary>
		/// Gets the item group.
		/// Used for grouping during autosort.
		/// </summary>
		public int ItemGroup
		{
			get
			{
				if (this.baseItemInfo == null)
					return 0;

				var iclass = this.baseItemInfo.ItemClass.ToUpperInvariant();
				var idx = ItemClassMap.Select((val, index) => new { val, index }).Where(m => m.val.ItemClass == iclass).Select(m => m.index);
				return idx.Any() ? idx.First() : 0;
			}
		}

		#endregion Item Properties

		#region Item Public Static Methods


		/// <summary>
		/// Generates a new seed that can be used on a new item
		/// </summary>
		/// <returns>New seed from 0 to 0x7FFF</returns>
		public static int GenerateSeed()
			// The seed values in the player files seem to be limitted to 0x00007fff or less.
			=> random.Next(0x00007fff);

		#endregion Item Public Static Methods

		/// <summary>
		/// Creates an empty item
		/// </summary>
		/// <returns>Empty Item structure</returns>
		public Item MakeEmptyItem()
		{
			Item newItem = new Item();
			newItem.beginBlockCrap1 = this.beginBlockCrap1;
			newItem.endBlockCrap1 = this.endBlockCrap1;
			newItem.beginBlockCrap2 = this.beginBlockCrap2;
			newItem.endBlockCrap2 = this.endBlockCrap2;
			newItem.BaseItemId = string.Empty;
			newItem.prefixID = string.Empty;
			newItem.suffixID = string.Empty;
			newItem.relicID = string.Empty;
			newItem.relic2ID = string.Empty;
			newItem.RelicBonusId = string.Empty;
			newItem.RelicBonus2Id = string.Empty;
			newItem.Seed = GenerateSeed();
			newItem.Var1 = this.var1;
			newItem.Var2 = var2Default;
			newItem.PositionX = -1;
			newItem.PositionY = -1;

			return newItem;
		}

		/// <summary>
		/// Makes a new item based on the passed item id string
		/// </summary>
		/// <param name="baseItemId">base item id of the new item</param>
		/// <returns>Empty Item structure based on the passed item string</returns>
		public Item MakeEmptyCopy(string baseItemId)
		{
			if (string.IsNullOrEmpty(baseItemId))
				throw new ArgumentNullException(baseItemId, "The base item ID cannot be NULL or Empty.");

			Item newItem = MakeEmptyItem();
			newItem.BaseItemId = baseItemId;

			return newItem;
		}

		/// <summary>
		/// Tell if the item is modified
		/// </summary>
		public bool IsModified { get; set; }

		/// <summary>
		/// Makes a duplicate of the item
		/// VillageIdiot - Added option to keep item seed.
		/// </summary>
		/// <param name="keepItemSeed">flag on whether we are keeping our item seed or creating a new one.</param>
		/// <returns>New Duplicated item</returns>
		public Item Duplicate(bool keepItemSeed)
		{
			Item newItem = (Item)this.MemberwiseClone();
			if (!keepItemSeed)
				newItem.Seed = GenerateSeed();

			newItem.PositionX = -1;
			newItem.PositionY = -1;
			newItem.IsModified = true;

			return newItem;
		}

		/// <summary>
		/// Clones the item
		/// </summary>
		/// <returns>A new clone of the item</returns>
		public Item Clone()
		{
			Item newItem = (Item)this.MemberwiseClone();
			newItem.IsModified = true;
			return newItem;
		}

		/// <summary>
		/// Pops all but one item from the stack.
		/// </summary>
		/// <returns>Returns popped items as a new Item stack</returns>
		public Item PopAllButOneItem()
		{
			if (!this.DoesStack || this.StackSize < 2)
				return null;

			// make a complete copy then change a few things
			Item newItem = (Item)this.MemberwiseClone();

			newItem.StackSize = this.StackSize - 1;
			newItem.Var1 = 0;
			newItem.Seed = GenerateSeed();
			newItem.PositionX = -1;
			newItem.PositionY = -1;
			newItem.IsModified = true;

			this.IsModified = true;

			this.StackSize = 1;

			return newItem;
		}

		#region GearType related

		/// <summary>
		/// <see cref="GearType"/> of this Item.
		/// </summary>
		/// <remarks>return <see cref="GearType.Undefined"/> if not a piece of gear</remarks>
		public GearType GearType
			=> ItemClassGearTypeMap
				.Where(m => m.ItemClass.Equals(this.ItemClass, StringComparison.OrdinalIgnoreCase))
				.Select(m => m.Type).FirstOrDefault();

		internal static ReadOnlyCollection<(string ItemClass, GearType Type)> ItemClassGearTypeMap = new[]
		{
			("ARMORJEWELRY_AMULET", GearType.Amulet),
			("ARMORJEWELRY_RING", GearType.Ring),
			("ITEMARTIFACT", GearType.Artifact),
			("ARMORPROTECTIVE_LOWERBODY", GearType.Leg),
			("ARMORPROTECTIVE_FOREARM", GearType.Arm),
			("ARMORPROTECTIVE_HEAD", GearType.Head),
			("ARMORPROTECTIVE_UPPERBODY", GearType.Torso),
			("WEAPONARMOR_SHIELD", GearType.Shield),
			("WEAPONMELEE_AXE", GearType.Axe),
			("WEAPONMELEE_MACE", GearType.Mace),
			("WEAPONMELEE_SWORD", GearType.Sword),
			("WEAPONHUNTING_BOW", GearType.Bow),
			("WEAPONHUNTING_RANGEDONEHAND", GearType.Thrown),
			("WEAPONMAGICAL_STAFF", GearType.Staff),
			("WEAPONHUNTING_SPEAR",GearType.Spear),
		}.ToList().AsReadOnly();

		#endregion

		#region IsRelicAllowed

		/// <summary>
		/// Tells if <paramref name="relicBaseItemId"/> is allowed.
		/// </summary>
		/// <param name="relicBaseItemId"></param>
		/// <returns></returns>
		public bool IsRelicAllowed(string relicBaseItemId)
			=> IsRelicAllowed(this, relicBaseItemId);

		/// <summary>
		/// Tells if <paramref name="relicItem"/> is allowed.
		/// </summary>
		/// <param name="relicItem"></param>
		/// <returns></returns>
		public bool IsRelicAllowed(Item relicItem)
			=> IsRelicAllowed(this, relicItem);

		/// <summary>
		/// Tells if <paramref name="relicItem"/> is allowed on <paramref name="item"/>.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="relicItem"></param>
		/// <returns></returns>
		public static bool IsRelicAllowed(Item item, Item relicItem)
		{
			// relicItem a relic ?
			if (!relicItem.IsRelic) throw new ArgumentException("Must be a relic or a charm!", nameof(relicItem));

			var itemGearType = item.GearType;

			if (itemGearType == GearType.Undefined) return false;

			// Is it allowed ?
			if (!TryGetAllowedGearTypes(relicItem, out var relicAllowedGearTypes))
				return false;

			return relicAllowedGearTypes.HasFlag(itemGearType);
		}

		/// <summary>
		/// Tells if <paramref name="relicBaseItemId"/> is allowed on <paramref name="item"/>.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="relicBaseItemId"></param>
		/// <returns></returns>
		public static bool IsRelicAllowed(Item item, string relicBaseItemId)
		{
			var itemGearType = item.GearType;

			if (itemGearType == GearType.Undefined) return false;

			// Is it allowed ?
			if (!TryGetAllowedGearTypes(relicBaseItemId, out var relicAllowedGearTypes))
				return false;

			return relicAllowedGearTypes.HasFlag(itemGearType);
		}

		#endregion

		#region TryGetAllowedGearTypes

		/// <summary>
		/// Try to get <paramref name="relicItem"/> allowed <see cref="GearType"/>
		/// </summary>
		/// <param name="relicItem"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static bool TryGetAllowedGearTypes(Item relicItem, out GearType types)
		{
			types = GearType.Undefined;

			// relicItem a relic ?
			if (!relicItem.IsRelic) return false;

			return TryGetAllowedGearTypes(relicItem.BaseItemId, out types);
		}

		/// <summary>
		/// Try to get allowed <see cref="GearType"/> for that <paramref name="relicBaseItemId"/>
		/// </summary>
		/// <param name="relicBaseItemId"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static bool TryGetAllowedGearTypes(string relicBaseItemId, out GearType types)
		{
			types = GearType.Undefined;

			if (string.IsNullOrWhiteSpace(relicBaseItemId)) return false;

			// Find GearType
			var map = RelicAndCharmExtension.RelicAndCharmMap
				.Where(m => m.FileName.Equals(Path.GetFileName(relicBaseItemId), StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			if (map.Value == RelicAndCharm.Unknown) return false;

			// Found
			types = map.Types;

			return true;
		}

		#endregion

	}
}
