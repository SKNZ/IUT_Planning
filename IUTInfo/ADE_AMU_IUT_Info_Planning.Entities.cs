/*	
	This file is part of IUTInfo.

	IUTInfo is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	IUTInfo is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with IUTInfo.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;

namespace IUTInfo
{
    public partial class ADE_AMU_IUT_Info_Planning
    {
        public class EntityData
        {
            public EntityData(uint id, string name)
            {
                Id = id;
                Name = name;
            }

            public uint Id { get; set; }
            public string Name { get; set; }
        }

        public EntityData SelectedEntity
        {
            get
            {
                return Entities[SelectedEntityID];
            }
        }

        private uint _selectedEntityID;
                     
        public uint SelectedEntityID
        {
            get
            {
                return _selectedEntityID;
            }
            set
            {
                _selectedEntityID = value;
            }
        }

        public Dictionary<uint, EntityData> Entities           = new List<EntityData>
                                                                            {
                                                                                new EntityData( 8385, "An. 1 - Gr. 1A"        ),
                                                                                new EntityData( 8386, "An. 1 - Gr. 1B"        ),
                                                                                new EntityData( 8387, "An. 1 - Gr. 2A"        ),
                                                                                new EntityData( 8388, "An. 1 - Gr. 2B"        ),
                                                                                new EntityData( 8389, "An. 1 - Gr. 3A"        ),
                                                                                new EntityData( 8390, "An. 1 - Gr. 3B"        ),
                                                                                new EntityData( 8391, "An. 1 - Gr. 4A"        ),
                                                                                new EntityData( 8392, "An. 1 - Gr. 4B"        ),
                                                                                new EntityData( 8393, "An. 1 - Gr. 5A"        ),
                                                                                new EntityData( 8394, "An. 1 - Gr. 5B"        ),
                                                                                new EntityData( 8400, "An. 2 - Gr. 1A"        ),
                                                                                new EntityData( 8401, "An. 2 - Gr. 1B"        ),
                                                                                new EntityData( 8402, "An. 2 - Gr. 2A"        ),
                                                                                new EntityData( 8403, "An. 2 - Gr. 2B"        ),
                                                                                new EntityData( 8404, "An. 2 - Gr. 3A"        ),
                                                                                new EntityData( 8405, "An. 2 - Gr. 3B"        ),
                                                                                new EntityData( 6445, "LP"                    ),
                                                                                new EntityData(  333, "BERNE Michel"          ),
                                                                                new EntityData(10974, "BERTHET Anne-Charlotte"),
                                                                                new EntityData( 8953, "BERTRAND Luc"          ),
                                                                                new EntityData( 1321, "BOITARD Didier"        ),
                                                                                new EntityData(  230, "BONHOMME Christian"    ),
                                                                                new EntityData( 1320, "BROCHE Martine"        ),
                                                                                new EntityData( 1126, "CACCHIA Marie claude"  ),
                                                                                new EntityData( 1244, "CASALI Alain"          ),
                                                                                new EntityData(  983, "CICCHETTI Rosine"      ),
                                                                                new EntityData( 2100, "CREUX Philippe"        ),
                                                                                new EntityData( 2038, "FERRY Jonas"           ),
                                                                                new EntityData(  479, "GAITAN Patricia"       ),
                                                                                new EntityData(  100, "HURST Kristen"         ),
                                                                                new EntityData( 3224, "KIAN Yavar"            ),
                                                                                new EntityData(  152, "LAKHAL Lotfi"          ),
                                                                                new EntityData( 1312, "LAPORTE Marc"          ),
                                                                                new EntityData( 2008, "MAKSSOUD Christine"    ),
                                                                                new EntityData( 1731, "MARTIN-NEVOT Mickael"  ),
                                                                                new EntityData( 1300, "MONNET Marlène"        ),
                                                                                new EntityData( 1467, "NEDJAR Sebastien"      ),
                                                                                new EntityData(  411, "PAIN BARRE Cyril"      ),
                                                                                new EntityData( 2149, "PESCI Fabien"          ),
                                                                                new EntityData( 8954, "PONS Olivier"          ),
                                                                                new EntityData(  386, "RISCH Vincent"         ),
                                                                                new EntityData( 4494, "ROGUET Emmanuel"       ),
                                                                                new EntityData( 9352, "SCHONER Corinne"       ),
                                                                                new EntityData( 1439, "SLEZAK Eileen"         ),
                                                                                new EntityData(  803, "VAQUIERI Josee"        ),
                                                                                new EntityData( 2062, "YAHI Safa"             ),
                                                                                new EntityData( 2070, "WOOD David"            ),
                                                                                new EntityData( 8414, "Salle A"               ),
                                                                                new EntityData( 8415, "Salle B"               ),
                                                                                new EntityData( 8416, "Salle C"               ),
                                                                                new EntityData( 8417, "Salle D"               ),
                                                                                new EntityData( 8418, "Salle E"               )
                                                                            }.ToDictionary(data => data.Id);
    }
}
