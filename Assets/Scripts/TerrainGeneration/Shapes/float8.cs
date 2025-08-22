namespace Stubblefield.Shapes
{
    [System.Serializable]
    public struct float8
    {
        public float v0, v1, v2, v3, v4, v5, v6, v7;

        public unsafe float this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 8)
                    throw new System.ArgumentException("index must be between[0...7]");
#endif
                fixed (float8* array = &this) { return ((float*)array)[index]; }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint)index >= 8)
                    throw new System.ArgumentException("index must be between[0...7]");
#endif
                fixed (float* array = &v0) { array[index] = value; }
            }
        }

        public override string ToString()
        {
            return $"v0:{v0} v1:{v1} v2:{v2} v3:{v3} v4:{v4} v5:{v5} v6:{v6} v7:{v7}";
        }
    }
}