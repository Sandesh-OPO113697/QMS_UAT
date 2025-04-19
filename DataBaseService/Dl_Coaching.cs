using QMS.Encription;

namespace QMS.DataBaseService
{
    public class Dl_Coaching
    {
        private readonly string _con;
        private readonly DL_Encrpt _enc;
        private readonly DLConnection _dcl;
        public Dl_Coaching(DL_Encrpt dL_Encrpt, DLConnection dL)
        {

            _enc = dL_Encrpt;
            _dcl = dL;
        }
    }
}
