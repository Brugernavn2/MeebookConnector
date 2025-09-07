using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeebookConnector
{
    public class AulaPlanRepository : BaseRepository
    {
        public AulaPlanRepository()
        {

        }

        public void Insert(AulaPlanItem item)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<AulaPlanItem>("plans");
            col.Insert(item);
        }

        public AulaPlanItem? GetById(string id)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<AulaPlanItem>("plans");
            return col.FindById(id);
        }

        public void InsertOrUpdate(AulaPlanItem item)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<AulaPlanItem>("plans");
            col.Update(item);
        }

        public List<AulaPlanItem> GetAll()
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<AulaPlanItem>("plans");
            return col.FindAll().ToList();
        }

        public void Delete(string id)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<AulaPlanItem>("plans");
            col.Delete(id);
        }
    }
}
