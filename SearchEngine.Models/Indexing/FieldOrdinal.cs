namespace SearchEngine.Models.Indexing
{
    public static class FieldOrdinal
    {
        public static Field OrdinalToField(short ord) =>
            ord switch
            {
                0 => Field.Title,
                1 => Field.Description,
                2 => Field.Tags,
                _ => throw new ArgumentOutOfRangeException(nameof(ord)),
            };

        public static short FieldToOrdinal(Field field) =>
            field switch
            {
                Field.Title => 0,
                Field.Description => 1,
                Field.Tags => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(field)),
            };
    }
}
