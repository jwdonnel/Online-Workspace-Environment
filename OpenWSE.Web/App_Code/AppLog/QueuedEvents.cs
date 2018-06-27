using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class QueuedEvents {
    private string _eventname;
    private string _eventcomment;
    private Exception _e;
    private HttpContext _context;

    public QueuedEvents(string eventname, string eventcomment, Exception e, HttpContext context) {
        _eventname = eventname;
        _eventcomment = eventcomment;
        _e = e;
        _context = context;
    }

    public string EventName {
        get { return _eventname; }
    }

    public string EventComment {
        get { return _eventcomment; }
    }

    public Exception e {
        get { return _e; }
    }

    public HttpContext Context {
        get { return _context; }
    }

}