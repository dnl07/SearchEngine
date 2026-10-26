# SearchEngine

## What is this engine for?

This engine is a lightweight, fast and easy-to-use full-text search engine that runs as an HTTP service and can be integrated into your own applications.

It keeps several named indexes in memory. Each index declares its own fields, field weights and stopwords. Documents are ranked by relevance (BM25) and found even with typos, by prefix or by a part of a word. Filtering, faceting and paging are left to the application.

## Features

- Several named indexes, searchable one by one or together
- A schema per index: fields with weights, stopwords per language
- BM25 ranking, weighted per field
- Exact, prefix, infix and fuzzy matching (edit distance)
- All terms of a query must match
- Normalization of case, diacritics and `ß`, compound words like `T-Shirt`
- Thread-safe, no persistence: the application rebuilds the indexes after a restart

## API Endpoints

### Indexes
- ```PUT /indexes/{index}``` - Creates an index or changes its settings.
- ```GET /indexes/{index}``` - Returns the settings and the number of documents
- ```DELETE /indexes/{index}``` - Deletes an index

Index names consist of 1 to 64 lower-case letters, digits, `-` and `_`.

Settings:
```json
{
  "fields": [
    { "name": "tags", "weight": 3, "lengthNormalization": 0 },
    { "name": "notes", "weight": 0.5 }
  ],
  "stopwords": {
    "languages": ["de", "en"],
    "exclude": ["s", "m", "l"]
  }
}
```
`lengthNormalization` (BM25 `b`, 0 to 1, default 0.75) sets how much longer values lower the score. Keep the default for free text and use 0 for fields that hold a list of values like tags or colors, so an item with many tags is not ranked below one with a single tag.

`languages` selects the built-in stopword lists (Snowball, `de` and `en`), `exclude` removes words from them.

### Documents
- ```PUT /indexes/{index}/documents``` - Replaces all documents at once. Searches keep using the old documents until the new ones are indexed.
- ```POST /indexes/{index}/documents``` - Adds or replaces the given documents
- ```DELETE /indexes/{index}/documents/{id}``` - Deletes a document

Documents:
```json
[
  {
    "id": "string",
    "fields": {
      "tags": ["Winter", "Sport"],
      "notes": ["string"]
    }
  }
]
```
Ids are chosen by the caller. Fields that are not declared in the settings are rejected. A batch is applied completely or not at all.

### Search
- ```POST /indexes/{index}/search``` - Searches an index
- ```POST /multi-search``` - Searches several indexes, the results are grouped per index

Request:
```json
{ "query": "rote jacke", "limit": 20, "explain": false }
```
Without `limit` all hits are returned. `total` is always the number of all hits.

Response:
```json
{
  "total": 1,
  "hits": [{ "id": "string", "score": 0.0 }],
  "tookMs": 0
}
```
With `explain: true` every hit lists for each term the matched token, how it matched (`Exact`, `Prefix`, `Infix`, `Fuzzy`), its number of typos and the score per field.

Multi-search:
```json
{ "queries": [{ "index": "clothing", "query": "jacke", "limit": 10 }] }
```
```json
{ "results": [{ "index": "clothing", "total": 0, "hits": [] }], "tookMs": 0 }
```

### Health
- ```GET /health``` - Returns basic status of the search engine
