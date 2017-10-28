namespace GunRack
{
    public enum PutGunResult
    {
        UnknownError = 0,
        Success,
        InvalidIndex,
        NotCarrying,
        WeaponNotAllowed,
        SlotOccupied,
        RackFull
    }

    public enum TakeGunResult
    {
        UnknownError = 0,
        Success,
        InvalidIndex,
        WeaponNotFound
    }
}