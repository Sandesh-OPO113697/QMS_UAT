using QMS.Encription;

namespace QMS.DataBaseService
{
    public class Dl_UpdateManagement
    {

        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public Dl_UpdateManagement( DL_Encrpt dL_Encrpt, DLConnection dL)
        {
          
            _enc = dL_Encrpt;
            _dcl = dL;
        }
    }
}
