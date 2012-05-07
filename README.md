Tableize
========

Takes normal html and inlines css style and converts elements to old school tables for use in sending email.  This way you can write html templates and maintain your sanity.

Tableize turns

```<p class="big-text">Hi Mom</p>```

into

```<table cellspacing="0" cellpadding="0"><tr><td align="center" style="font-size:30px">Hi Mom</td></tr></table>```

It's super high perfomance and typically takes only a few milliseconds to process, so it's safe to use in your bulk emailing services.