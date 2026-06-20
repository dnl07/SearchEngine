namespace SearchEngine.Core.Documents
{
    public class TokenRegistry
    {
        private readonly Dictionary<int, string> _idToToken = new Dictionary<int, string>();
        private readonly Dictionary<string, int> _tokenToId = new Dictionary<string, int>();

        private int _idCounter = 0;

        public int Count => _tokenToId.Count;

        public string GetToken(int id)
        {
            if (_idToToken.TryGetValue(id, out var token))
            {
                return token;
            }
            return "";
        }

        public int GetIdOfToken(string token)
        {
            if (_tokenToId.TryGetValue(token, out var id))
            {
                return id;
            }
            return -1;
        }

        /// <summary>
        /// Adds the token to the N-Gram-Index.
        /// </summary>
        /// <returns>The corresponding id of the token if successfully added, -1 otherwise.</returns>
        public int Add(string token)
        {
            if (_tokenToId.TryAdd(token, _idCounter))
            {
                _idToToken.Add(_idCounter, token);
                _idCounter++;

                return _idCounter - 1;
            }
            return -1;
        }

        public void Remove(string token)
        {
            var tokenId = _tokenToId[token];
            _tokenToId.Remove(token);
            _idToToken.Remove(tokenId);
        }
    }
}
