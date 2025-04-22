//repo is not needed in this project as we are using database

using System;
using System.Collections.Generic;
using System.Linq;

namespace EComWebNewApplication.Repositories
{
    public class Repository<T>
    {
        private List<T> _items = new List<T>();

        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }

        public IEnumerable<T> GetAll()
        {
            return _items;
        }


        //step 3 adding a linq query
        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return _items.Where(predicate);
        }
    }
}

