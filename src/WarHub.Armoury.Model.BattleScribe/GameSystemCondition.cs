// WarHub licenses this file to you under the MIT license.
// See LICENSE file in the project root for more information.

namespace WarHub.Armoury.Model.BattleScribe
{
    public class GameSystemCondition : Condition, IGameSystemCondition
    {
        private IGameSystemContext _context;

        public GameSystemCondition(BattleScribeXml.Condition xml)
            : base(xml)
        {
        }

        public IGameSystemContext Context
        {
            get { return _context; }
            set
            {
                if (Set(ref _context, value))
                {
                    ; //TODO set context of IdLinks
                }
            }
        }
    }
}
