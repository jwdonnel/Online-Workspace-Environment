/// <summary>
///     Summary description for MonthConverter
/// </summary>
public class MonthConverter
{
    public static string ToStringMonth(int month)
    {
        string x;
        switch (month)
        {
            case 1:
                x = "January";
                break;
            case 2:
                x = "February";
                break;
            case 3:
                x = "March";
                break;
            case 4:
                x = "April";
                break;
            case 5:
                x = "May";
                break;
            case 6:
                x = "June";
                break;
            case 7:
                x = "July";
                break;
            case 8:
                x = "August";
                break;
            case 9:
                x = "September";
                break;
            case 10:
                x = "October";
                break;
            case 11:
                x = "November";
                break;
            case 12:
                x = "December";
                break;
            default:
                x = "January";
                break;
        }
        return x;
    }

    public static int ToIntMonth(string month)
    {
        int x;
        switch (month)
        {
            case "January":
                x = 1;
                break;
            case "February":
                x = 2;
                break;
            case "March":
                x = 3;
                break;
            case "April":
                x = 4;
                break;
            case "May":
                x = 5;
                break;
            case "June":
                x = 6;
                break;
            case "July":
                x = 7;
                break;
            case "August":
                x = 8;
                break;
            case "September":
                x = 9;
                break;
            case "October":
                x = 10;
                break;
            case "November":
                x = 11;
                break;
            case "December":
                x = 12;
                break;
            default:
                x = 1;
                break;
        }
        return x;
    }
}