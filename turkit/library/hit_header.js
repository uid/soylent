function setCookie(name, value, days, path, domain, secure) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime()+(days*24*60*60*1000));
		var expires = date
	}
  var curCookie = name + "=" + escape(value) +
      ((expires) ? "; expires=" + expires.toGMTString() : "") +
      ((path) ? "; path=" + path : "") +
      ((domain) ? "; domain=" + domain : "") +
      ((secure) ? "; secure" : "");
  document.cookie = curCookie;
}
function getCookie(name) {
  var dc = document.cookie;
  var prefix = name + "=";
  var begin = dc.indexOf("; " + prefix);
  if (begin == -1) {
    begin = dc.indexOf(prefix);
    if (begin != 0) return null;
  } else
    begin += 2;
  var end = document.cookie.indexOf(";", begin);
  if (end == -1)
    end = dc.length;
  return unescape(dc.substring(begin + prefix.length, end));
}

function unescapeURL(s) {
    return decodeURIComponent(s.replace(/\+/g, "%20"))
}

function getURLParams() {
    var params = {}
    var m = window.location.href.match(/[\\?&]([^=]+)=([^&#]*)/g)
    if (m) {
        for (var i = 0; i < m.length; i++) {
            var a = m[i].match(/.([^=]+)=(.*)/)
            params[unescapeURL(a[1])] = unescapeURL(a[2])
        }
    }
    return params
}

function swap(o, i1, i2) {
    var temp = o[i1]
    o[i1] = o[i2]
    o[i2] = temp
}

function shuffle(a) {
    for (var i = 0; i < a.length; i++) {
        swap(a, i, randomIndex(a.length))
    }
    return a
}

function randomIndex(n) {
    return Math.floor(Math.random() * n)
}

function randomizeClass(selector) {
	if (!selector) selector = $('.random')
    var r = selector;
	if (r.length > 1) {
		$(shuffle(r.after('<div/>').next().get())).each(function (i) {
			$(this).after(r[i]).remove()
		})
	}
}

$(function () {
    var params = getURLParams()
    if (params.workerId && (-1 != "___BLOCK_WORKERS___".indexOf(params.workerId))) {
    	$('body').empty().append('<h2>Please return this HIT</h2><p>You have done nothing wrong. However, for some reason, the requester for this HIT does not want you to complete it. This is probably because you have already participated in a group of HITs that are building on each other, and the requester wants to ensure that a variety of people work on the job. You may still be able to participate in future HITs in this process.</p><p><small>NOTE: This message is a temporary fix; we hope that Mechanical Turk itself will allow us to block specific workers from particular HITs, so that they do not show up under "HITs Available To You". Given this and other factors, Mechanical Turk requesters generally do not care how many HITs you return.</small></p><p><b>Sorry for the inconvenience.</b></p>')    	
    	return
    }
    if (params.assignmentId) {
        if (params.assignmentId == "ASSIGNMENT_ID_NOT_AVAILABLE") {
            $('input').attr("disabled", "true")
            $('textarea').attr("disabled", "true")
            $('button').attr("disabled", "true")
        } else {
        	if ($('*[name]').length < 2) {
        		$('input[type=submit]').attr('name', 'submit')
        	}
        	if ($('*[name]').length < 2) {
        		$('#assignmentId').after('<input type="hidden" name="default" value="default"></input>')
        	}
        }
        $('#assignmentId').attr('value', params.assignmentId)
        $('form').attr('method', 'POST')
    }
    if (params.turkSubmitTo) {
        $('form').attr('action', params.turkSubmitTo + '/mturk/externalSubmit')
    }
    
    // add a form-validation function to the submit buttons
    // ___FORM_VALIDATOR___
	
	randomizeClass();

	// log
	var isPreview = (params.assignmentId == "ASSIGNMENT_ID_NOT_AVAILABLE") ? 1 : 0;
	if (params.assignmentId) {	// if it's a hit on either the sandbox or real MTurk, not just opened on the hard drive
		$.ajax( {
			url: 'http://people.csail.mit.edu/msbernst/dev/hitcounter/hit.php?page=___PAGE_NAME___&preview=' + isPreview,
			dataType: 'jsonp'
		});
	}
})