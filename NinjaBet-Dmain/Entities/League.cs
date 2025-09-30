namespace NinjaBet_Dmain.Entities
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public League(int id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public League() { }
    }
}
