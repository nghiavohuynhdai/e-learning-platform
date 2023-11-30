namespace Cursus.Entities
{
    public class CartDatabaseSettings:ICartDatabaseSettings
    {
        public string CartCollectionName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string QuizAnswerCollectionName { get; set; } = string.Empty;
    }
    public interface ICartDatabaseSettings
    {
        public string CartCollectionName { get; set; }
        public string QuizAnswerCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
