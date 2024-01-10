using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MiniBots
{
    public class MiniBotsContext : DbContext
    {
        public DbSet<MiniBot> MiniBots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Database/mini-bots.db");
        }
    }

    [Table("miniBots")]
    public class MiniBot
    {
        [Key]
        public int Id { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(64)]
        public string Code { get; set; }

        [StringLength(64)]
        public string Storage { get; set; }

        public MiniBot()
        {
        }
    }

    public class DatabaseManager
    {
        private MiniBotsContext _context;

        public DatabaseManager()
        {
            _context = new MiniBotsContext();
            _context.Database.EnsureCreated();
        }

        public MiniBot? GetMiniBotByName(string name)
        {
            // Select all records using a LINQ query
            List<MiniBot> miniBots = [.. _context.MiniBots.Where(miniBot => miniBot.Name == name)];

            if (miniBots.Count == 0)
            {
                return null;
            }

            return miniBots[0];
        }

        public List<MiniBot> GetMiniBots()
        {
            // Select all records
            List<MiniBot> miniBots = [.. _context.MiniBots.ToList()];

            return miniBots;
        }

        public void CreateMiniBotIfNotExists(string name, string code)
        {
            MiniBot? miniBot = GetMiniBotByName(name);
            if (miniBot != null)
            {
                miniBot.Code = code;
                miniBot.Storage = "";
                _context.MiniBots.Update(miniBot);
            }
            else
            {
                MiniBot newMiniBot = new MiniBot
                {
                    Name = name,
                    Code = code,
                    Storage = ""
                };

                _context.MiniBots.Add(newMiniBot);
            }

            _context.SaveChanges();
        }

        public string GetStorage(int id)
        {
            MiniBot? miniBot = _context.MiniBots.Find(id);

            if (miniBot == null)
            {
                throw new Exception("MiniBot not found");
            }

            return miniBot.Storage;
        }

        /// <summary>
        ///   Set the storage of a bot.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="append"> If true, append the data to the existing storage. </param>
        public void SetStorage(int id, string data, bool append = false)
        {
            MiniBot? miniBot = _context.MiniBots.Find(id);

            if (miniBot == null)
            {
                throw new Exception("MiniBot not found");
            }

            if (append)
            {
                miniBot.Storage += data;
            }
            else
            {
                miniBot.Storage = data;
            }

            _context.SaveChanges();
        }

        public void DeleteMiniBot(int id)
        {
            MiniBot? miniBot = _context.MiniBots.Find(id);

            if (miniBot == null)
            {
                throw new Exception("MiniBot not found");
            }

            _context.MiniBots.Remove(miniBot);
            _context.SaveChanges();
        }
    }
}