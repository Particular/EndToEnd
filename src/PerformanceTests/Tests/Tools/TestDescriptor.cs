namespace Tests.Tools
{
    using Tests.Permutations;

    public class TestDescriptor
    {
        public Permutation Permutation { get; set; }
        public string ProjectAssemblyPath { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ProjectAssemblyDirectory { get; set; }
    }
}