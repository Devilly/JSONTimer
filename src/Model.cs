using System.Collections.Generic;

public readonly struct Activity {
	public string Name { get; init; }
	public IList<Specification> Specifications { get; init; }
}

public enum SpecificationType {
    Exercise,
	Rest,
    Recuperate
}

public readonly struct Specification {
    public SpecificationType Type { get; init; }
	public string Name { get; init; }
	public int Repetitions { get; init; }
	public int Duration { get; init; }
	public int Rest { get; init; }
    public string RestName { get; init; }
}
