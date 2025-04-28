using System;
using System.Collections.Generic;

namespace Base {

    public class ItemNotFoundException : Exception {
        public ItemNotFoundException() : base() { }
        public ItemNotFoundException(string message) : base(message) { }
        public ItemNotFoundException(List<string> messages) : base(messages.Count > 0 ? messages[0] : "") { }
        public ItemNotFoundException(string message, Exception inner) : base(message, inner) { }
    }


}
