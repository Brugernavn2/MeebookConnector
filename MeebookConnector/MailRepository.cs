using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeebookConnector
{
    public class MailRepository : BaseRepository
    {
        public void Insert(MailAddressModel mail)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<MailAddressModel>("mailList");
            col.Insert(mail);
        }

        public List<MailAddressModel> GetAll()
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<MailAddressModel>("mailList");
            return col.FindAll().ToList();
        }

        public void Clear()
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<MailAddressModel>("mailList");
            col.DeleteAll();
        }
    }
}
