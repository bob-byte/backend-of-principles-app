using SET.Shared.Models;

namespace SET.WebAPI.Models;

public class SaveUserNameRequest
{
    public string UserName { get; set; }
    public DateTime LastModified { get; set; }
}

public class SaveMainSloganRequest
{
    public string MainSlogan { get; set; }
    public DateTime LastModified { get; set; }
}

public class SaveMissionRequest
{
    public string Mission { get; set; }
    public DateTime LastModified { get; set; }
}

public class SaveGenderRequest
{
    public Gender Gender { get; set; }
    public DateTime LastModified { get; set; }
}
