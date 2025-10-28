using System;
using System.Collections.Generic;

namespace QH.Physics {
    public interface ISimulation {
        List<MassObject> MassList {
            get;
        }
        List<ConnectionBase> ConnectionList {
            get;
        }

        void Update(Single deltaTime);
    }
}