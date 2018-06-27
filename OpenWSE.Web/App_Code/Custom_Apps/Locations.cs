using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


[Serializable]
public class Locations
{
    private string _postalCode;
    private string _latitude;
    private string _longitude;

    public Locations(string postalCode, string latitude, string longitude)
    {
        _postalCode = postalCode;
        _latitude = latitude;
        _longitude = longitude;
    }

    public string PostalCode
    {
        get { return _postalCode; }
    }

    public string Latitude
    {
        get { return _latitude; }
    }

    public string Longitude
    {
        get { return _longitude; }
    }
}