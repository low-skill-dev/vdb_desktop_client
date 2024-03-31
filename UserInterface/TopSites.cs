﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterface;
internal static class TopSites
{
	public static readonly string[] Sites = { @"startnext.com", @"geek.com", @"researchgate.net", @"amazon.com.br", @"plaza.rakuten.co.jp", @"is.gd", @"hostgator.com", @"health.com", @"myaccount.google.com", @"yoursite.com", @"seroundtable.com", @"getresponse.com", @"spotify.com", @"allmusic.com", @"livescience.com", @"lg.com", @"spectrum.ieee.org", @"abcnews.go.com", @"firstdata.com", @"hangouts.google.com", @"stats.wp.com", @"residentadvisor.net", @"steemit.com", @"note.mu", @"neilpatel.com", @"sxsw.com", @"we.tl", @"prnewswire.com", @"businesswire.com", @"netbeans.org", @"checkpoint.com", @"onlinelibrary.wiley.com", @"nfl.com", @"denverpost.com", @"europe1.fr", @"lh3.googleusercontent.com", @"penguinrandomhouse.com", @"buff.ly", @"spreaker.com", @"download.microsoft.com", @"evernote.com", @"windows.microsoft.com", @"google.com.br", @"copyblogger.com", @"docker.com", @"translate.google.com", @"opinionator.blogs.nytimes.com", @"digitaltrends.com", @"example.com", @"yummly.com", @"scribd.com", @"flattr.com", @"thelancet.com", @"drive.google.com", @"ted.com", @"puu.sh", @"npr.org", @"nypost.com", @"bhphotovideo.com", @"discordapp.com", @"theguardian.com", @"popularmechanics.com", @"bitcointalk.org", @"wsj.com", @"tandfonline.com", @"buzzsprout.com", @"networkadvertising.org", @"thingiverse.com", @"1.bp.blogspot.com", @"homedepot.com", @"api.whatsapp.com", @"wp.me", @"gist.github.com", @"flickr.com", @"jstor.org", @"venturebeat.com", @"vmware.com", @"collegehumor.com", @"imgur.com", @"amazon.it", @"4shared.com", @"stats.g.doubleclick.net", @"cisco.com", @"nginx.com", @"pitchfork.com", @"google.be", @"schema.org", @"i.imgur.com", @"lh5.ggpht.com", @"examiner.com", @"abebooks.com", @"osha.gov", @"avvo.com", @"cloud.google.com", @"google.se", @"zeit.de", @"hp.com", @"kraken.com", @"thehill.com", @"launchpad.net", @"g.co", @"a.co", @"zillow.com", @"artsandculture.google.com", @"t.co", @"fonts.gstatic.com", @"fiverr.com", @"blogs.scientificamerican.com", @"npmjs.com", @"us.battle.net", @"digg.com", @"digitalocean.com", @"pinterest.ca", @"webmasters.googleblog.com", @"booking.com", @"amazon.es", @"i.redd.it", @"ticketmaster.com", @"edx.org", @"telegraph.co.uk", @"instructables.com", @"dailymotion.com", @"amazon.co.jp", @"groups.google.com", @"foxnews.com", @"trends.google.com", @"reddit.com", @"mail.google.com", @"sendspace.com", @"bugs.chromium.org", @"gmail.com", @"apnews.com", @"anchor.fm", @"vr.google.com", @"namecheap.com", @"bandcamp.com", @"rebrand.ly", @"developers.google.com", @"kickstarter.com", @"raw.githubusercontent.com", @"wix.com", @"spiegel.de", @"help.apple.com", @"lh6.googleusercontent.com", @"michigan.gov", @"eonline.com", @"space.com", @"upi.com", @"xkcd.com", @"cdn.jsdelivr.net", @"nature.com", @"dafont.com", @"help.ubuntu.com", @"access.redhat.com", @"boardgamegeek.com", @"louvre.fr", @"huffingtonpost.com", @"bit.ly", @"cse.google.com", @"photos.app.goo.gl", @"skype.com", @"google.com.au", @"raspberrypi.org", @"hostinger.com", @"code.google.com", @"ietf.org", @"msn.com", @"chris.pirillo.com", @"pewinternet.org", @"yahoo.com", @"google.nl", @"justgiving.com", @"time.com", @"flic.kr", @"unsplash.com", @"baidu.com", @"goo.gl", @"buzzfeednews.com", @"foursquare.com", @"discord.me", @"udacity.com", @"stocktwits.com", @"axios.com", @"lesechos.fr", @"tensorflow.org", @"cell.com", @"analytics.google.com", @"deepl.com", @"store.steampowered.com", @"dashlane.com", @"getpocket.com", @"support.google.com", @"addthis.com", @"google.co.in", @"yandex.com", @"bloomberg.com", @"marthastewart.com", @"pexels.com", @"pinterest.co.uk", @"m.youtube.com", @"smile.amazon.com", @"aboutads.info", @"l.facebook.com", @"adage.com", @"maps.googleapis.com", @"1drv.ms", @"helpx.adobe.com", @"amazon.co.uk", @"amazon.in", @"youtu.be", @"forbes.com", @"download.macromedia.com", @"nydailynews.com", @"mlb.com", @"moma.org", @"techsmith.com", @"medium.com", @"pinterest.com", @"salesforce.com", @"angelfire.com", @"db.tt", @"blog.hubspot.com", @"khanacademy.org", @"eclipse.org", @"prnt.sc", @"bbc.co.uk", @"expedia.com", @"freshbooks.com", @"pnas.org", @"washingtonpost.com", @"money.cnn.com", @"fortune.com", @"myfitnesspal.com", @"sellfy.com", @"google-analytics.com", @"houzz.com", @"lefigaro.fr", @"cdn.shopify.com", @"ibm.com", @"1.gravatar.com", @"goo.gle", @"patents.google.com", @"google.co.jp", @"ameblo.jp", @"verizon.com", @"metro.co.uk", @"news.google.com", @"kotaku.com", @"docs.wixstatic.com", @"reuters.com", @"podbean.com", @"bitbucket.org", @"behance.net", @"news.nationalgeographic.com", @"gumroad.com", @"lifehack.org", @"pixiv.net", @"flipboard.com", @"motherboard.vice.com", @"1.envato.market", @"weibo.com", @"google.co.nz", @"symfony.com", @"patheos.com", @"profiles.google.com", @"stumbleupon.com", @"cancerresearchuk.org", @"calendly.com", @"shareasale.com", @"docs.microsoft.com", @"coinmarketcap.com", @"automattic.com", @"pt.slideshare.net", @"yandex.ru", @"j.mp", @"laughingsquid.com", @"popsci.com", @"reacts.ru", @"gov.uk", @"music.apple.com", @"ifttt.com", @"fastcompany.com", @"ja-jp.facebook.com", @"apps.apple.com", @"snopes.com", @"ebay.com", @"zazzle.com", @"bbc.com", @"blog.feedspot.com", @"redbubble.com", @"amzn.asia", @"bmj.com", @"store.google.com", @"w3.org", @"instagram.com", @"lynda.com", @"dev.mysql.com", @"wired.com", @"rollingstone.com", @"event.on24.com", @"repubblica.it", @"python.org", @"huffingtonpost.co.uk", @"vanmiubeauty.com", @"play.google.com", @"academic.oup.com", @"0.gravatar.com", @"google.de", @"google.ca", @"billboard.com", @"a2hosting.com", @"families.google.com", @"i0.wp.com", @"www-01.ibm.com", @"vizio.com", @"disqus.com", @"lifehacker.com", @"ubuntu.com", @"last.fm", @"googletagmanager.com", @"connect.facebook.net", @"creativecommons.org", @"lh4.googleusercontent.com", @"blogs.adobe.com", @"presseportal.de", @"audible.com", @"material.io", @"sketchfab.com", @"androidauthority.com", @"ssl.google-analytics.com", @"pcworld.com", @"francetvinfo.fr", @"design.google", @"static.wixstatic.com", @"paypal.me", @"nbc.com", @"squarespace.com", @"get.adobe.com", @"stackoverflow.com", @"zalo.me", @"googlewebmastercentral.blogspot.com", @"yelp.com", @"change.org", @"s0.wp.com", @"chrome.google.com", @"vanityfair.com", @"mixi.jp", @"sophos.com", @"thenextweb.com", @"giphy.com", @"ticketportal.cz", @"amzn.to", @"whatsapp.com", @"cia.gov", @"keep.google.com", @"theverge.com", @"search.google.com", @"gplus.to", @"obsproject.com", @"unesco.org", @"support.mozilla.org", @"theatlantic.com", @"inkscape.org", @"amzn.com", @"speakerdeck.com", @"g.page", @"idealo.de", @"tumblr.com", @"asus.com", @"unity3d.com", @"science.sciencemag.org", @"developer.android.com", @"use.typekit.net", @"abc.com", @"techcrunch.com", @"fb.com", @"logitech.com", @"independent.co.uk", @"fda.gov", @"eff.org", @"shutterstock.com", @"4.bp.blogspot.com", @"dribbble.com", @"zen.yandex.ru", @"maxcdn.bootstrapcdn.com", @"zoom.us", @"dl.dropbox.com", @"livestream.com", @"marriott.com", @"nhs.uk", @"storage.googleapis.com", @"googleadservices.com", @"arxiv.org", @"slashgear.com", @"about.fb.com", @"aclu.org", @"heise.de", @"colorado.edu", @"realvnc.com", @"orcid.org", @"skfb.ly", @"newyorker.com", @"dol.gov", @"get.google.com", @"pixabay.com", @"smashwords.com", @"adf.ly", @"s.w.org", @"yadi.sk", @"blog.naver.com", @"apis.google.com", @"propublica.org", @"bol.com", @"ftc.gov", @"huffpost.com", @"googleads.g.doubleclick.net", @"ranker.com", @"gitlab.com", @"zdnet.com", @"2.bp.blogspot.com", @"lh3.ggpht.com", @"constantcontact.com", @"cambridge.org", @"news.bbc.co.uk", @"mobile.twitter.com", @"wa.me", @"media.giphy.com", @"siteground.com", @"google.ie", @"skillshare.com", @"dl.dropboxusercontent.com", @"fonts.google.com", @"platform.twitter.com", @"apps.facebook.com", @"sublimetext.com", @"pwc.com", @"cnn.com", @"coursera.org", @"cloudflare.com", @"adweek.com", @"m.me", @"services.google.com", @"android.com", @"sutterhealth.org", @"journals.sagepub.com", @"9to5mac.com", @"songkick.com", @"blogger.com", @"ea.com", @"apple.co", @"tunein.com", @"eventim.de", @"google.pt", @"tinyurl.com", @"lulu.com", @"worldbank.org", @"aws.amazon.com", @"filezilla-project.org", @"united.com", @"buffer.com", @"nationalgeographic.com", @"cnet.com", @"mp.weixin.qq.com", @"untappd.com", @"google.gr", @"maps.gstatic.com", @"online.wsj.com", @"snapchat.com", @"photos.google.com", @"istockphoto.com", @"use.fontawesome.com", @"cbs.com", @"codecanyon.net", @"ancestry.com", @"youtube-nocookie.com", @"player.vimeo.com", @"google.pl", @"s7.addthis.com", @"pandora.com", @"ravelry.com", @"ad.doubleclick.net", @"3.bp.blogspot.com", @"gopro.com", @"pond5.com", @"gmpg.org", @"messenger.com", @"notion.so", @"sites.google.com", @"tools.ietf.org", @"s-media-cache-ak0.pinimg.com", @"hulu.com", @"mailchimp.com", @"freepik.com", @"teespring.com", @"sciencedirect.com", @"support.apple.com", @"img.youtube.com", @"esa.int", @"audacityteam.org", @"docs.google.com", @"google.cn", @"eventbrite.co.uk", @"refinery29.com", @"vox.com", @"upwork.com", @"udemy.com", @"freewebs.com", @"nodejs.org", @"linktr.ee", @"market.android.com", @"infusionsoft.com", @"starwars.com", @"twitter.com", @"guardian.co.uk", @"copyright.gov", @"neh.gov", @"thesun.co.uk", @"qz.com", @"edition.cnn.com", @"developer.mozilla.org", @"box.net", @"linkedin.com", @"forms.office.com", @"nytimes.com", @"gstatic.com", @"abc.net.au", @"youtube.com", @"springer.com", @"metmuseum.org", @"issuu.com", @"secure.gravatar.com", @"postmates.com", @"drift.com", @"doi.org", @"wattpad.com", @"cdnjs.cloudflare.com", @"accessify.com", @"columbia.edu", @"target.com", @"themarthablog.com", @"dx.doi.org", @"news.yahoo.com", @"ctt.ec", @"lemonde.fr", @"twitch.tv", @"elmundo.es", @"form.jotform.com", @"pastebin.com", @"psychologytoday.com", @"7-zip.org", @"producthunt.com", @"walmart.com", @"steamcommunity.com", @"stock.adobe.com", @"s3.amazonaws.com", @"kstatic.googleusercontent.com", @"themeforest.net", @"adssettings.google.com", @"espn.com", @"journals.plos.org", @"picasaweb.google.com", @"git-scm.com", @"economist.com", @"pewresearch.org", @"adobe.ly", @"iheart.com", @"googleblog.blogspot.com", @"ft.com", @"dailycaller.com", @"pbs.twimg.com", @"meetup.com", @"paypal.com", @"books.google.com", @"vimeo.com", @"bitly.com", @"redcross.org", @"lh5.googleusercontent.com", @"gofundme.com", @"gimp.org", @"problogger.net", @"gleam.io", @"blip.tv", @"economictimes.indiatimes.com", @"france24.com", @"cdbaby.com", @"amazon.fr", @"news.harvard.edu", @"arstechnica.com", @"wetransfer.com", @"businessinsider.com", @"i2.wp.com", @"m.facebook.com", @"business.facebook.com", @"maps.google.com", @"mailchi.mp", @"redbull.com", @"searchengineland.com", @"imdb.com", @"envato.com", @"loc.gov", @"leparisien.fr", @"static.googleusercontent.com", @"payhip.com", @"kiva.org", @"humblebundle.com", @"chromium.org", @"codex.wordpress.org", @"drupal.org", @"blogs.windows.com", @"blog.us.playstation.com", @"scratch.mit.edu", @"dev.to", @"ko-fi.com", @"apple.com", @"japantimes.co.jp", @"archive.org", @"privacy.microsoft.com", @"google.es", @"eur-lex.europa.eu", @"healthline.com", @"snip.ly", @"earth.google.com", @"500px.com", @"marketingplatform.google.com", @"ign.com", @"goodreads.com", @"monster.com", @"wordpress.org", @"google.ch", @"canva.com", @"azure.microsoft.com", @"dropbox.com", @"franchising.com", @"entrepreneur.com", @"microsoft.com", @"amazon.com.au", @"ikea.com", @"mega.nz", @"youcaring.com", @"google.co.za", @"indiewire.com", @"podcasts.apple.com", @"socialmediatoday.com", @"hbr.org", @"sourceforge.net", @"appstore.com", @"open.spotify.com", @"buymeacoffee.com", @"ouest-france.fr", @"brookings.edu", @"patreon.com", @"maps.google.co.jp", @"uber.com", @"podcasts.google.com", @"greenpeace.org", @"tripadvisor.com", @"surveymonkey.com", @"gizmodo.com", @"code.jquery.com", @"buzzfeed.com", @"stadt-bremerhaven.de", @"support.microsoft.com", @"digiday.com", @"amnestyusa.org", @"moz.com", @"cafepress.com", @"css-tricks.com", @"soundcloud.com", @"events.google.com", @"etsy.com", @"link.springer.com", @"coinbase.com", @"gravatar.com", @"mediafire.com", @"1.usa.gov", @"w3schools.com", @"adwords.google.com", @"themify.me", @"bizjournals.com", @"overcast.fm", @"bloglovin.com", @"gsuite.google.com", @"video.google.com", @"iconfinder.com", @"squareup.com", @"marketwatch.com", @"cdn.ampproject.org", @"discord.gg", @"latimes.com", @"usatoday.com", @"t.ly", @"tools.google.com", @"google.fr", @"imore.com", @"blog.google", @"www8.hp.com", @"google.it", @"login.microsoftonline.com", @"slideshare.net", @"mixcloud.com", @"support.cloudflare.com", @"hbo.com", @"opera.com", @"g1.globo.com", @"pixlr.com", @"variety.com", @"globalsign.com", @"relapse.com", @"indiegogo.com", @"elegantthemes.com", @"hollywoodreporter.com", @"forms.gle", @"fbi.gov", @"en.advertisercommunity.com", @"i1.wp.com", @"creativemarket.com", @"jamanetwork.com", @"prntscr.com", @"rockpapershotgun.com", @"amazon.com", @"calendar.google.com", @"google.cz", @"developers.facebook.com", @"policies.google.com", @"thinkwithgoogle.com", @"waze.com", @"jetbrains.com", @"business.google.com", @"ads.google.com", @"s3-eu-west-1.amazonaws.com", @"maps.google.co.nz", @"gum.co", @"ericsson.com", @"who.int", @"bluehost.com", @"pagead2.googlesyndication.com", @"flaticon.com", @"en-gb.facebook.com", @"eventbrite.com", @"de-de.facebook.com", @"xbox.com", @"activecampaign.com", @"web.facebook.com", @"slate.com", @"bitpay.com", @"sfgate.com", @"tf1.fr", @"gnu.org", @"amazon.ca", @"lenovo.com", @"ustream.tv", @"bigthink.com", @"whitehouse.gov", @"codepen.io", @"tesla.com", @"chronicle.com", @"dailymail.co.uk", @"adobe.com", @"feeds.feedburner.com", @"foodnetwork.com", @"funnyordie.com" };
}
