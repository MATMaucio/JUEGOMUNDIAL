using System;
using System.Collections.Generic;

[Serializable]
public class POIData
{
    public string nombre;
    public double lat;
    public double lon;
    public string descripcion;
}

[Serializable]
public class POIList
{
    public List<POIData> paradas;
}