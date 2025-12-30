using EldenRingSim.DB;

namespace EldenRingSim.Repositories
{
    public interface IWeaponRepository : IRepository<Weapons>
    {
        Task<IEnumerable<Weapons>> GetByCategoryAsync(string category);
        Task<IEnumerable<Weapons>> GetByWeightRangeAsync(double minWeight, double maxWeight);
        Task<IEnumerable<Weapons>> GetWeaponsMeetingRequirementsAsync(
            int strength, int dexterity, int intelligence, int faith);
        
        Task<Weapons?> GetByNameAsync(string name);
    }
}