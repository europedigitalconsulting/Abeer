using Abeer.Shared.Functional;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Abeer.Data.Repositories
{
    public class TeamRepository
    {
        private readonly FunctionalDbContext _context;

        public TeamRepository(FunctionalDbContext context)
        {
            _context = context;
        }

        public Task<IList<Team>> GetTeams()
        {
            return Task.Run(() => _context.Teams.ToList());
        }

        public Task<Team> GetTeam(Guid id)
        {
            return Task.Run(() => _context.Teams.FirstOrDefault(c => c.Id == id));
        }

        public Task Update(Team Team)
        {
            return Task.Run(() => _context.Teams.Update(Team));
        }

        public Task<Team> Add(Team Team)
        {
            return Task.Run(() => _context.Teams.Add(Team));
        }

        public Task<Team> FirstOrDefault(Expression<Func<Team, bool>> p)
        {
            return Task.Run(() => _context.Teams.FirstOrDefault(p));
        }

        public Task<IList<Team>> Where(Expression<Func<Team, bool>> p)
        {
            return Task.Run(() => _context.Teams.Where(p));
        }

        public void Remove(Team Team)
        {
            _context.Teams.Remove(Team.Id);
        }

        public Task<bool> Any(Expression<Func<Team, bool>> p) => Task.Run(() => _context.Teams.Any(p));

        public Task Delete(Guid id)
        {
            return Task.Run(() => _context.Teams.Remove(id));
        }
    }
}