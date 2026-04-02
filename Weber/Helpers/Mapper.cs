using Neotoma.Contract.Models;
using Weber.Models;

namespace Weber.Helpers;

public static class Mapper
{
    public static FileObject ToFileObject(this FileObjectNotify value)
    {
        return new()
        {
            Data = value.Data,
            Hash = value.Hash,
            Id = value.Id,
            Name = value.Name,
            Description = value.Description,
        };
    }
}
