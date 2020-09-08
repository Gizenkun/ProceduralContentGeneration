using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IDugeonBuilder
{
    void Create(IDugeonData dugeonData, Action<Map> completeCb);
}
