using System;
using System.Collections.Generic;
using System.Text;

namespace Khonshu.Otpavitel
{
    public static class HelperForEmail
    {
        public static String GetHtmlBody(String author,String text)
        {
           
            String htmlBody = $$"""
<div style="background:#0a0a0a; padding:20px; border-radius:10px; border:1px solid #0fcf3d; font-family:sans-serif; max-width:400px; color:#fff;">
    <h2 style="color:#0fcf3d; margin:0 0 15px 0; font-size:14px; text-transform:uppercase;">Новое сообщение</h2>
    
    <div style="margin-bottom:15px;">
        <span style="color:#666; font-size:11px;">ОТПРАВИТЕЛЬ:</span>
        <div style="font-weight:bold; font-size:18px;">{{author}}</div>
    </div>
    
    <div style="background:#1a1a1a; padding:15px; border-radius:5px; border-left:3px solid #0fcf3d;">
        <span style="color:#666; font-size:11px;">ТЕКСТ:</span>
        <div style="color:#ccc; margin-top:5px; font-style:italic;">"{{text}}"</div>
    </div>
</div>
""";

            return htmlBody;
        }
    }
}
