using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalDiscordBot.Classes
{
    public static class Snoop
    {
        public static string Snoopify(string text)
        {
            return text
                .Replace(" and ", " n' ")
                .Replace("ity ", "itizzle ")
                .Replace("cisco ", "kieco ")
                .Replace(" released ", " busted out ")
                .Replace("ing ", "in ")
                .Replace(" little ", " lil ")
                .Replace(" that ", " dat ")
                .Replace(" very ", " straight up ")
                .Replace("ive ", "izzle")
                .Replace(" to ", " ta ")
                .Replace(" the ", " da ")
                .Replace(" popular ", " ghettofab ")
                .Replace(" some ", " shitload ")
                .Replace(" largest ", " phattest ")
                .Replace(" companies ", " g-units ")
                .Replace(" company ", " g-unit ")
                .Replace(" is ", " be ")
                .Replace(" an ", " a ")
                .Replace(" site ", " joint ")
                .Replace(" of a ", " cold ass lil ")
                .Replace(" better ", " betta ")
                .Replace(" understand ", " KNOW ")
                .Replace(" said ", " holla'd ")
                .Replace(" for ", " fo' ")
                .Replace(" more ", " mo' ")
                .Replace(" trouble ", " shit ")
                .Replace(" such as ", " like fuckin ")
                .Replace(" a ", " a cold ass lil ")
                .Replace(" you can ", " yo slick ass ")
                .Replace(" worked ", " hit dat shiznit ")
                .Replace("er ", "a ")
                .Replace(" blog ", " snoop bloggy blogg ")
                .Replace("eds ", "edz ")
                .Replace("ition ", "izzle ")
                .Replace(" he ", " da thug ")
                .Replace("ess ", "izz ")
                .Replace(" discussion ", " rap ")
                .Replace(" really ", " straight up ")
                .Replace(" my ", " mah ")
                .Replace(" people ", " playas ")
                .Replace(" got ", " gots ")
                .Replace("nal ", "nistic ")
                .Replace(" about ", " bout ")
                .Replace("le ", "lez ")
                .Replace(" spent ", " dropped like ")
                .Replace(" visiting ", " hittin' up ")
                .Replace("ds ", "dz ")
                .Replace(" hack", " jack")
                .Replace(" interested ", " horny bout ")
                .Replace(" with a ", " wit a thugged out ")
                .Replace(" with ", " wit ")
                .Replace(" because ", " cuz ")
                .Replace(" fun", " funk")
                .Replace(" myself ", " ma dirty ass ")
                .Replace(" how ", " how the fuck ")
                .Replace(" to ", " ta ")
                .Replace(" into ", " tha fuck into ")
                .Replace(" me ", " mah crazy ass ")
                .Replace(" after ", " afta ")
                .Replace(" everything ", " every last muthafuckin thang ")
                .Replace("an ", "a ")
                .Replace(" comes from ", " be reppin ")
                .Replace("ing ", "in ")
                .Replace(" some ", " shitload ")
                .Replace(" planet ", " hood ")
                .Replace(" think ", " thinkin ")
                .Replace(" them ", " dem wild ass muthafuckas ")
                .Replace(" wife ", " hoe ")
                .Replace("enn ", "izz ")
                .Replace("en ", "izz ")
                .Replace(" northern ", " uptown ")
                .Replace(" at ", " all up in ")
                .Replace(" enjoy ", " trip off ")
                .Replace(" shoot ", " blast")
                .Replace(" you ", " tha fuck you ")
                .Replace(" in ", " up in the ")
                .Replace(" the da ", " the ");
        }

        public static string ToSnoopification(this string text)
        {
            return Snoopify(text);
        }
    }
}
