namespace ComponentHost
{
    public struct ComponentResolution
    {
        public string RequestedLibrary;
        public string RequestedComponent;
        public bool ResolvedLibrary;
        public string ResolvedLibraryPath;
        public string[] LibraryCandidates;
    }    
}
