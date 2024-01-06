using Watson.ORM.Core;
using DatabaseWrapper.Core;

namespace MiniBots
{
    // Apply attributes to your class
    [Table("miniBots")]
    public class MiniBot
    {
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        [Column("name", false, DataTypes.Nvarchar, 64, false)]
        public string Name { get; set; }

        [Column("code", false, DataTypes.Nvarchar, 64, false)]
        public string Code { get; set; }

        [Column("storage", false, DataTypes.Nvarchar, 64, false)]
        public string Storage { get; set; }

        // Parameter-less constructor is required
        public MiniBot()
        {
        }
    }
}