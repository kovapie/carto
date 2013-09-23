namespace carto.Models
{
    public class CmdbItemCategory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public CmdbItemCategory(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}