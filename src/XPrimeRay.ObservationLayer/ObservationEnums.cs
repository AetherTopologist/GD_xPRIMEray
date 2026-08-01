namespace XPrimeRay.ObservationLayer;

public enum SceneClass
{
    Unknown = 0,
    RegressionFixture = 1,
    BenchmarkFixture = 2,
    DiagnosticChamber = 3,
    InteractiveScene = 4,
    CanonicalExperiment = 5,
    BeautyDemonstration = 6
}

public enum ObservationDataType
{
    Unknown = 0,
    UInt8 = 1,
    UInt16 = 2,
    Int32 = 3,
    Float32 = 4,
    Float2 = 5,
    Float3 = 6,
    Boolean = 7,
    ProbeOutcomeCode = 8,
    InstrumentObservationRecord = 9,
    TransportEventRecord = 10
}

public enum ObservationDomain
{
    DensePixelPlane = 1,
    SparseRayRecord = 2,
    RegionRecord = 3,
    TransportEventStream = 4
}

public enum InstrumentTier
{
    TransportRecord = 1,
    DirectObservation = 2,
    DerivedGeometric = 3,
    FieldObservation = 4,
    SemanticClassification = 5
}

public enum ObservationValidity
{
    Unknown = 0,
    Valid = 1,
    RequiresCompleteFrame = 2,
    MissingChannel = 3,
    TypeMismatch = 4,
    InvalidDescriptor = 5
}

public enum VisualizationRangePolicy
{
    Fixed = 1,
    AutoFrame = 2,
    AutoDataset = 3
}

public enum TransportEventKind
{
    Unknown = 0,
    FirstInteraction = 1,
    Terminal = 2,
    BoundaryEntry = 3,
    BoundaryExit = 4,
    BoundaryRemap = 5,
    FieldSourceContribution = 6
}
