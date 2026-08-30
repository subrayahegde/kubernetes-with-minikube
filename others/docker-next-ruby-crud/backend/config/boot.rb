ENV['BUNDLE_GEMFILE'] ||= File.expand_path('../Gemfile', __dir__)

require 'bundler/setup' if File.exist?(ENV['BUNDLE_GEMFILE'])
require 'bootsnap/setup' if ENV['BUNDLE_GEMFILE'] && File.exist?(ENV['BUNDLE_GEMFILE'])
