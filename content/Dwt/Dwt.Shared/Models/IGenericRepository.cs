using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwt.Shared.Models;

/// <summary>
/// Generic interface for repositories.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IGenericRepository<T>
{
    T Create(T t);

    T? GetByID(string id);

    IEnumerable<T> GetAll();

    bool Update(T t);

    bool Delete(T t);
}
