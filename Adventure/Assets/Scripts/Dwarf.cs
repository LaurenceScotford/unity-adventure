// Dwarf
// Represents a single dwarf

[System.Serializable]
public class Dwarf
{
    // === PROPERTIES ===

    // The dwarf's current location
    public string DwarfLocation { get; set; }

    // The dwarf's previous location
    public string OldDwarfLocation { get; set; }

    // Keeps track of whether this dwarf has seen the player avatar
    public bool SeenPlayer { get; set; }

    // Keeps track of whether the dwarf is alive or dead
    public DwarfStatus Status { get; set; }

    // === CONSTRUCTOR ===

    public Dwarf()
    {
        DwarfLocation = null;
        OldDwarfLocation = null;
        SeenPlayer = false;
        Status = DwarfStatus.ALIVE;
    }
}

// Whether a dwarf is alive or dead
public enum DwarfStatus { DEAD, ALIVE };