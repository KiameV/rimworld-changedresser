using System.Text;

namespace ChangeDresser.UI.Util
{
    struct HSL
    {
        public float h;
        public float s;
        public float l;

        public HSL(HSL hsl)
        {
            this.h = hsl.h;
            this.s = hsl.s;
            this.l = hsl.l;
        }

        public HSL(float h = 0, float s = 0, float l = 0)
        {
            this.h = h;
            this.s = s;
            this.l = l;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && this.GetType() == obj.GetType())
            {
                HSL hsl = (HSL)obj;
                return this.h == hsl.h && this.s == hsl.s && this.l == hsl.l;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            sb.Append(this.h);
            sb.Append(",");
            sb.Append(this.s);
            sb.Append(",");
            sb.Append(this.l);
            sb.Append("]");
            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(HSL l, HSL r)
        {
            return l.h == r.h && l.s == r.s && l.l == r.l;
        }

        public static bool operator !=(HSL l, HSL r)
        {
            return l.h != r.h || l.s != r.s || l.l != r.l;
        }
    }
}
