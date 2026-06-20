using SearchEngine.Core.Search;

namespace SearchEngine.Core
{
    public class SearchEngineCore
    {
        public readonly Engine SearchEngine;

        public SearchEngineCore()
        {
            SearchEngine = new Engine();
        }
    }
}
