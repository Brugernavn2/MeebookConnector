using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeebookConnector
{
    public class StudentRepository : BaseRepository
    {
        public StudentRepository()
        {

        }

        public void Insert(Student item)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Student>("students");
            col.Insert(item);
        }

        public Student? GetById(string id)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Student>("students");
            return col.FindById(id);
        }

        public void InsertOrUpdate(Student item)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Student>("students");
            col.Upsert(item);
        }

        public List<Student> GetAll()
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Student>("students");
            return col.FindAll().ToList();
        }

        public void Delete(string id)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Student>("students");
            col.Delete(id);
        }

        public void Clear()
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Student>("students");
            col.DeleteAll();
        }
    }
}
