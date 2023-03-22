using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.Contracts.Categories
{
    public record struct AddCategoryRequest(string Name);

    public record struct AddCategoryResponse(string Id, string Name);
}