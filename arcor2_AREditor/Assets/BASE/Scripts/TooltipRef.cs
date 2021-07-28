using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipRef : Base.Singleton<TooltipRef>
{
    public GameObject Tooltip;
    public TMPro.TextMeshProUGUI Text;
    public TMPro.TMP_Text SubDescription;    
}
