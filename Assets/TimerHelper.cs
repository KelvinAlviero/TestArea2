using UnityEngine;

namespace Extras
{
    [System.Serializable]
    public class SimpleLongSave
    {
        [SerializeField] long value;
        public virtual long Value
        {
            get => value; set
            {
                this.value = value;
            }
        }
    
        public virtual void Flush() { }
        
    }

    
}