using System;

namespace PlayOnline.FFXI {

  [Flags]
  public enum Job : ushort {
    None = 0x0000,
    All  = 0xFFFE,
    // Specific
    WAR  = 0x0002,
    MNK  = 0x0004,
    WHM  = 0x0008,
    BLM  = 0x0010,
    RDM  = 0x0020,
    THF  = 0x0040,
    PLD  = 0x0080,
    DRK  = 0x0100,
    BST  = 0x0200,
    BRD  = 0x0400,
    RNG  = 0x0800,
    SAM  = 0x1000,
    NIN  = 0x2000,
    DRG  = 0x4000,
    SMN  = 0x8000,
  }

  [Flags]
  public enum Race : ushort {
    None           = 0x0000,
    All            = 0x01FE,
    // Specific
    HumeMale       = 0x0002,
    HumeFemale     = 0x0004,
    ElvaanMale     = 0x0008,
    ElvaanFemale   = 0x0010,
    TarutaruMale   = 0x0020,
    TarutaruFemale = 0x0040,
    Mithra         = 0x0080,
    Galka          = 0x0100,
    // Race Groups
    Hume           = 0x0006,
    Elvaan         = 0x0018,
    Tarutaru       = 0x0060,
    // Gender Groups (with Mithra = female, and Galka = male)
    Male           = 0x012A,
    Female         = 0x00D4,
  }

  [Flags]
  public enum EquipmentSlot : ushort {
    // Slot Groups
    None   = 0x0000,
    Ears   = 0x3000,
    Rings  = 0x6000,
    All    = 0xFFFF,
    // Specific Slots
    Main   = 0x0001,
    Sub    = 0x0002,
    Ranged = 0x0004,
    Ammo   = 0x0008,
    Head   = 0x0010,
    Torso  = 0x0020,
    Hands  = 0x0040,
    Legs   = 0x0080,
    Feet   = 0x0100,
    Neck   = 0x0200,
    Waist  = 0x0400,
    LEar   = 0x0800,
    REar   = 0x1000,
    LRing  = 0x2000,
    RRing  = 0x4000,
    Back   = 0x8000,
  }

  public enum ItemSkill : byte {
    None             = 0x00,
    HandToHand       = 0x01,
    Dagger           = 0x02,
    Sword            = 0x03,
    GreatSword       = 0x04,
    Axe              = 0x05,
    GreatAxe         = 0x06,
    Scythe           = 0x07,
    PoleArm          = 0x08,
    Katana           = 0x09,
    GreatKatana      = 0x0a,
    Club             = 0x0b,
    Staff            = 0x0c,
    Bow              = 0x19,
    Marksmanship     = 0x1a,
    Thrown           = 0x1b,
    StringInstrument = 0x29,
    WindInstrument   = 0x2a,
    Fishing          = 0x30,
  }

  [Flags]
  public enum ItemFlags : ushort {
    None      = 0x0000,
    // Unknown Bits
    Flag00     = 0x0001,
    Flag01     = 0x0002,
    Flag02     = 0x0004,
    Flag03     = 0x0008,
    Flag04     = 0x0010,
    Flag05     = 0x0020,
    Flag06     = 0x0040,
    Flag07     = 0x0080,
    Flag08     = 0x0100,
    Flag09     = 0x0200,
    Flag10     = 0x0400,
    Flag11     = 0x0800,
    Flag12     = 0x1000,
    Flag13     = 0x2000,
    Flag14     = 0x4000,
    Flag15     = 0x8000,
    // Assumed Bits
    NoAuction = 0x0040,
    CanEquip  = 0x0800,
    Ex        = 0x6040,
    Rare      = 0x8000,
  }

}