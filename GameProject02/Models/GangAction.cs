using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameProject02.Models
{
    public enum GangAction
    {
        DisbandGang,          // حل العصابة
        ChangeGangData,       // تغيير الاسم، الرمز، الصورة
        TransferLeadership,   // نقل الزعامة
        UpgradeGangLevel,     // ترقية مستوى العصابة
        KickMember,           // طرد عضو
        PromoteMember,        // ترقية عضو
        DemoteMember,         // تنزيل رتبة عضو
        AcceptJoinRequest,    // قبول طلب انضمام
        InviteMember,         // دعوة عضو
        SpendGangCash,        // إنفاق نقد العصابة
        ManageSkills          // إدارة مهارات العصابة
    }
}
